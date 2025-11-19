namespace Booking.Application.Abstractions;

public interface ISeatLockService
{
    // This will talk to Valkey (Redis)
    // Returns true if lock acquired, false if already locked
    Task<bool> TryLockSeatAsync(Guid showtimeId, Guid seatId, Guid userId);
}