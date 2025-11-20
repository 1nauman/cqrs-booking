namespace Booking.Domain.Events;

public record BookingCreatedEvent(
    Guid ShowtimeId,
    Guid BookingId,
    Guid UserId,
    SeatReservedItem[] Seats // The list of seats
);

public record SeatReservedItem(Guid SeatId, string Row, int Number);