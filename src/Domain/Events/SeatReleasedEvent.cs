namespace Booking.Domain.Events;

public record SeatReleasedEvent(
    Guid ShowtimeId,
    Guid SeatId
);