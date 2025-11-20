using Booking.Domain.Enums;
using Booking.Domain.Events;
using Booking.Infrastructure.Persistence;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Booking.API.Worker;

public class ReservationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReservationCleanupService> _logger;

    public ReservationCleanupService(IServiceProvider serviceProvider, ILogger<ReservationCleanupService> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredReservations(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up reservations.");
            }

            // Run every 1 minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessExpiredReservations(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Define threshold (e.g., 2 minutes for demo purposes)
        var threshold = DateTime.UtcNow.AddMinutes(-2);

        // 1. Fetch Expired Bookings WITH their Items
        // We must use .Include() because BookingItems are in a separate table now
        var expiredBookings = await context.Bookings
            .Include(b => b.Items)
            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < threshold)
            .ToListAsync(ct);

        if (!expiredBookings.Any()) return;

        _logger.LogInformation($"Found {expiredBookings.Count} expired bookings.");

        foreach (var booking in expiredBookings)
        {
            // 2. Mark the Transaction as Expired
            booking.MarkAsExpired();

            var releasedSeatIds = new List<Guid>();

            // 3. Release the physical seats in Postgres
            foreach (var item in booking.Items)
            {
                // We need to find the Seat entity to change its status to 'Available'
                var seat = await context.Seats.FindAsync(new object[] { item.SeatId }, ct);

                if (seat != null)
                {
                    seat.Release(); // Domain method: Status = Available, ReserverId = null
                    releasedSeatIds.Add(seat.Id);
                }
            }

            // 4. Publish ONE Batch Event
            // This triggers the Consumer to update Mongo and SignalR in one go
            if (releasedSeatIds.Any())
            {
                await publishEndpoint.Publish(new BookingExpiredEvent(
                    booking.Id,
                    booking.ShowtimeId,
                    releasedSeatIds.ToArray()
                ), ct);

                _logger.LogInformation($"Released {releasedSeatIds.Count} seats for Booking {booking.Id}");
            }
        }

        // 5. Commit all changes to Postgres
        await context.SaveChangesAsync(ct);
    }
}