namespace Booking.Domain.Enums;

public enum SeatStatus
{
    Available = 1,
    Reserved = 2, // Locked temporarily (in Redis/Postgres)
    Sold = 3      // Payment confirmed
}