namespace Booking.Domain.Enums;

public enum BookingStatus
{
    Pending = 1,    // User clicked "Book", waiting for payment
    Confirmed = 2,  // Payment successful
    Expired = 3,    // User failed to complete the payment in time
    Failed = 4      // System error
}