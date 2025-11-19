using Booking.Application.Abstractions;
using Booking.Application.Commands;
using Booking.Domain.Events;
using Booking.Domain.Repositories;

using MassTransit;

namespace Booking.Application.Handlers;

public class ReserveSeatHandler : ICommandHandler<ReserveSeatCommand, bool>
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

    public async Task<bool> HandleAsync(ReserveSeatCommand command, CancellationToken ct)
    {
        // 1. THE GATEKEEPER (Valkey/Redis)
        // This is the atomic check. If 100 users hit this line, 99 get 'false'.
        var isLocked = await _seatLockService.TryLockSeatAsync(
            command.ShowtimeId,
            command.SeatId,
            command.UserId
        );

        if (!isLocked)
        {
            // Race condition lost. Fail fast.
            return false;
        }

        // 2. LOAD DATA (Postgres)
        var seat = await _seatRepository.GetByIdAsync(command.SeatId, ct);
        if (seat is null)
        {
            // Edge case: Seat doesn't exist in DB
            return false;
        }

        // 3. DOMAIN LOGIC
        // This updates the C# object state
        try
        {
            seat.Reserve(command.UserId);
        }
        catch (InvalidOperationException)
        {
            // Seat was already sold (hard confirmed)
            return false;
        }

        // Create the Booking record (History)
        var booking = new Domain.Entities.Booking(command.ShowtimeId, command.SeatId, command.UserId);
        await _bookingRepository.AddAsync(booking, ct);

        // 4. UPDATE REPOSITORY
        await _seatRepository.UpdateAsync(seat, ct);

        // 5. PUBLISH EVENT (Transactional Outbox)
        // Because we are inside a UnitOfWork, MassTransit will NOT send this to RabbitMQ yet.
        // It will insert a record into the 'OutboxMessage' table in Postgres.
        await _publishEndpoint.Publish(new SeatReservedEvent(
            seat.ShowtimeId,
            seat.Id,
            seat.Row,
            seat.Number,
            command.UserId,
            DateTime.UtcNow
        ), ct);

        // 6. COMMIT TRANSACTION
        // This saves: The Seat Update + The New Booking + The Outbox Message
        // All or nothing.
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}