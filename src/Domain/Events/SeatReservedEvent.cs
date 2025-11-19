namespace Booking.Domain.Events;

public record SeatReservedEvent(
    Guid ShowtimeId,
    Guid SeatId,
    string Row,
    int Number,
    Guid UserId,
    DateTime ReservedAt
);