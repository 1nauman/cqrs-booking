namespace Booking.Domain.Events;

public record SeatSoldEvent(
    Guid ShowtimeId,
    Guid SeatId
);