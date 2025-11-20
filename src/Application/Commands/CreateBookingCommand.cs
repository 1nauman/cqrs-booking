namespace Booking.Application.Commands;

public record CreateBookingCommand(
    Guid ShowtimeId,
    Guid[] SeatIds,
    Guid UserId);