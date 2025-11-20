using Booking.Domain.Enums;

namespace Booking.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShowtimeId { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation Property: One Booking has Many Items
    private readonly List<BookingItem> _items = new();
    public IReadOnlyCollection<BookingItem> Items => _items.AsReadOnly();

    private Booking() { }

    public Booking(Guid showtimeId, Guid userId, List<Seat> seats)
    {
        Id = Guid.NewGuid();
        ShowtimeId = showtimeId;
        UserId = userId;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        foreach (var seat in seats)
        {
            _items.Add(new BookingItem(Id, seat.Id, seat.Row, seat.Number));
        }
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