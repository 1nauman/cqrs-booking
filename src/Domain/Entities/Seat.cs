using Booking.Domain.Enums;

namespace Booking.Domain.Entities;

public class Seat
{
    public Guid Id { get; private set; }
    public Guid ShowtimeId { get; private set; }
    public string Row { get; private set; } = string.Empty;
    public int Number { get; private set; }
    public SeatStatus Status { get; private set; }

    // Who holds the lock? (Nullable because it might be Available)
    public Guid? CurrentReserverId { get; private set; }

    // Optimistic Concurrency Token (Crucial for Postgres)
    // If two threads try to update this row, one will fail because the version won't match.
    public uint Version { get; set; }

    private Seat() { }

    public Seat(Guid showtimeId, string row, int number)
    {
        Id = Guid.NewGuid();
        ShowtimeId = showtimeId;
        Row = row;
        Number = number;
        Status = SeatStatus.Available;
    }

    // Domain Logic: Reserve the seat
    public void Reserve(Guid userId)
    {
        if (Status == SeatStatus.Sold)
            throw new InvalidOperationException("Seat is already sold.");

        // Note: We don't check "Reserved" here strictly because 
        // Redis is our primary gatekeeper, but it's good practice.

        Status = SeatStatus.Reserved;
        CurrentReserverId = userId;
    }

    // Domain Logic: Confirm the sale
    public void ConfirmSale()
    {
        if (Status != SeatStatus.Reserved)
            throw new InvalidOperationException("Cannot sell a seat that isn't reserved.");

        Status = SeatStatus.Sold;
    }

    // Domain Logic: Release the seat (Expiry)
    public void Release()
    {
        Status = SeatStatus.Available;
        CurrentReserverId = null;
    }
}