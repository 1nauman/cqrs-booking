namespace Booking.Domain.Entities;

public class BookingItem
{
    public Guid Id { get; private set; }

    public Guid BookingId { get; private set; }

    public Guid SeatId { get; private set; }

    // Snapshot data: We store Row/Number here too, 
    // so if the cinema layout changes later, the historical booking is still accurate.
    public string Row { get; private set; }

    public int Number { get; private set; }

    private BookingItem() { }

    public BookingItem(Guid bookingId, Guid seatId, string row, int number)
    {
        Id = Guid.NewGuid();
        BookingId = bookingId;
        SeatId = seatId;
        Row = row;
        Number = number;
    }
}