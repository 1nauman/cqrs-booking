using Booking.Domain.Enums;

namespace Booking.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid ShowtimeId { get; private set; }
    public Guid SeatId { get; private set; }
    public Guid UserId { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Booking() { }

    public Booking(Guid showtimeId, Guid seatId, Guid userId)
    {
        Id = Guid.NewGuid();
        ShowtimeId = showtimeId;
        SeatId = seatId;
        UserId = userId;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
    }

    public void MarkAsExpired()
    {
        Status = BookingStatus.Expired;
    }
}