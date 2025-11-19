using Booking.Application.Abstractions;

using StackExchange.Redis;

namespace Booking.Infrastructure.Services;

public class SeatLockService : ISeatLockService
{
    private readonly IConnectionMultiplexer _redis;

    public SeatLockService(IConnectionMultiplexer redis)
    {
        ArgumentNullException.ThrowIfNull(redis);
        _redis = redis;
    }

    public async Task<bool> TryLockSeatAsync(Guid showtimeId, Guid seatId, Guid userId)
    {
        var db = _redis.GetDatabase();

        // Key format: "lock:showtime_id:seat_id"
        var key = $"lock:{showtimeId}:{seatId}";
        var value = userId.ToString();

        // SET key value NX EX 600
        // NX: Only set if Not Exists (Atomic Lock)
        // EX: Expire in 10 minutes (Safety net for Zombie reservations)
        return await db.StringSetAsync(key, value, TimeSpan.FromMinutes(10), When.NotExists);
    }
}