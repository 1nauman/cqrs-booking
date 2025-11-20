namespace Booking.Domain.Events;

public record SeatsReservedEvent(
    Guid ShowtimeId,
    Guid BookingId,
    Guid UserId,
    List<SeatReservedItem> Seats // The list of seats
);

public record SeatReservedItem(Guid SeatId, string Row, int Number);