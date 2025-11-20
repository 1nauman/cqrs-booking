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

        var threshold = DateTime.UtcNow.AddMinutes(-2);

        // Fetch expired bookings AND include their items
        var expiredBookings = await context.Bookings
            .Include(b => b.Items) // <--- Eager load items
            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < threshold)
            .ToListAsync(ct);

        if (!expiredBookings.Any()) return;

        foreach (var booking in expiredBookings)
        {
            _logger.LogInformation($"Expiring Booking {booking.Id} with {booking.Items.Count} seats.");

            booking.MarkAsExpired();

            // Release ALL seats associated with this booking
            foreach (var item in booking.Items)
            {
                var seat = await context.Seats.FindAsync(new object[] { item.SeatId }, ct);
                if (seat != null)
                {
                    seat.Release();

                    // Publish event to turn seat Green in UI
                    await publishEndpoint.Publish(new SeatReleasedEvent(
                        booking.ShowtimeId,
                        item.SeatId
                    ), ct);
                }
            }
        }

        await context.SaveChangesAsync(ct);
    }
}