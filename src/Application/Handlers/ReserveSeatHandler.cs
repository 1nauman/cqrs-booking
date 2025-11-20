using Booking.Application.Abstractions;
using Booking.Application.Commands;
using Booking.Domain.Enums;
using Booking.Domain.Events;
using Booking.Domain.Repositories;

using MassTransit;

namespace Booking.Application.Handlers;

public class ReserveSeatHandler : ICommandHandler<ReserveSeatsCommand, bool>
{
    private readonly ISeatLockService _seatLockService;
    private readonly ISeatRepository _seatRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public ReserveSeatHandler(ISeatLockService seatLockService, ISeatRepository seatRepository,
        IBookingRepository bookingRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
    {
        ArgumentNullException.ThrowIfNull(seatLockService);
        ArgumentNullException.ThrowIfNull(seatRepository);
        ArgumentNullException.ThrowIfNull(bookingRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(publishEndpoint);

        _seatLockService = seatLockService;
        _seatRepository = seatRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> HandleAsync(ReserveSeatsCommand command, CancellationToken ct)
    {
        // 0. Basic Input Validation
        if (command.SeatIds.Length == 0)
        {
            return false;
        }

        // 1. THE GATEKEEPER (Lock First)
        // We lock whatever the user sent. If they sent garbage, we lock garbage keys.
        // This is acceptable because it protects the DB from the thundering herd.
        var locked = await _seatLockService.TryLockSeatsAsync(command.ShowtimeId, command.SeatIds, command.UserId);

        if (!locked)
        {
            return false; // Someone else holds the lock
        }

        try
        {
            // 2. VERIFY (The Truth Check)
            // Fetch the actual seats from Postgres
            var seats = await _seatRepository.GetByIdsAsync(command.ShowtimeId, command.SeatIds, ct);

            // 3. VALIDATION LOGIC

            // Check A: Did we find all the seats requested?
            // If user sent 3 IDs, but DB only returned 2, one ID is fake/invalid.
            if (seats.Count != command.SeatIds.Length)
            {
                throw new InvalidOperationException("One or more seat IDs are invalid.");
            }

            // Check B: Are any seats already sold?
            // (Redis protects against concurrent 'Reservations', but 'Sold' is a permanent state in DB)
            if (seats.Any(s => s.Status == SeatStatus.Sold))
            {
                throw new InvalidOperationException("One or more seats are already sold.");
            }

            // 4. DOMAIN LOGIC (Update State)
            foreach (var seat in seats)
            {
                seat.Reserve(command.UserId);

                // Create Booking Record
                var booking = new Domain.Entities.Booking(command.ShowtimeId, seat.Id, command.UserId);
                await _bookingRepository.AddAsync(booking, ct);

                await _seatRepository.UpdateAsync(seat, ct);
            }

            // 5. PUBLISH EVENT
            // We can publish a bulk event or loop. For simplicity, let's loop or create a BulkEvent.
            // Ideally, create a 'SeatsReservedEvent' that takes a list.
            foreach (var seat in seats)
            {
                await _publishEndpoint.Publish(new SeatReservedEvent(
                    seat.ShowtimeId, seat.Id, seat.Row, seat.Number, command.UserId, DateTime.UtcNow
                ), ct);
            }

            // 6. COMMIT
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception)
        {
            // 7. COMPENSATE (Rollback)
            // If ANYTHING went wrong (Validation failed, DB error, Bug), 
            // we MUST release the Redis locks immediately so others can try.
            await _seatLockService.ReleaseLocksAsync(command.ShowtimeId, command.SeatIds);
            throw; // Re-throw to return 500 or 400 to client
        }
    }
}