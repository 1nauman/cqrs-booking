namespace Booking.Application.Abstractions;

public interface ISeatLockService
{
    // This will talk to Valkey (Redis)
    // Returns true if lock acquired, false if already locked
    Task<bool> TryLockSeatsAsync(Guid showtimeId, Guid[] seatIds, Guid userId);

    Task ReleaseLocksAsync(Guid showtimeId, Guid[] seatIds);
}