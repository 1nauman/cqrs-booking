namespace Booking.Domain.Events;

public record BookingExpiredEvent(
    Guid BookingId,
    Guid ShowtimeId,
    Guid[] SeatIds
);