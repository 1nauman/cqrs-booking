namespace Booking.Application.Commands;

public record ReserveSeatsCommand(
    Guid ShowtimeId,
    Guid[] SeatIds,
    Guid UserId);