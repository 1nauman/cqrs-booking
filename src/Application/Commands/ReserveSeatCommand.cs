namespace Booking.Application.Commands;

public record ReserveSeatCommand(
    Guid ShowtimeId,
    Guid SeatId,
    Guid UserId);