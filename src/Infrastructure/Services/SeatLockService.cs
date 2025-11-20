using Booking.Application.Abstractions;

using StackExchange.Redis;

namespace Booking.Infrastructure.Services;

public class SeatLockService : ISeatLockService
{
    private readonly IConnectionMultiplexer _valKeyStore;

    public SeatLockService(IConnectionMultiplexer valKeyStore)
    {
        ArgumentNullException.ThrowIfNull(valKeyStore);
        _valKeyStore = valKeyStore;
    }

    public async Task<bool> TryLockSeatsAsync(Guid showtimeId, Guid[] seatIds, Guid userId)
    {
        var db = _valKeyStore.GetDatabase();

        // Key format: "lock:showtime_id:seat_id"
        var keys = seatIds.Select(id => new RedisKey($"lock:{showtimeId}:{id}")).ToArray();
        var value = seatIds.Select(_ => new RedisValue(userId.ToString())).ToArray();

        // LUA SCRIPT: Atomic "Check and Multi-Set"
        // ARGV[1] = Expiry in seconds (600)
        // ARGV[2] = User ID
        // KEYS[...] = The seat keys
        var luaScript = """
                            -- 1. Check if ANY key already exists
                            for i, key in ipairs(KEYS) do
                                if redis.call('EXISTS', key) == 1 then
                                    return 0 -- Fail immediately, even if a single seat is taken
                                end
                            end
                            
                            -- 2. If we are here, all seats are free. Lock them all.
                            for i, key in ipairs(KEYS) do
                                redis.call('SET', key, ARGV[2], 'EX', ARGV[1])
                            end
                            
                            return 1 -- Success
                        """;

        var result = await db.ScriptEvaluateAsync(
            luaScript,
            keys,
            [new RedisValue("600"), new RedisValue(userId.ToString())] // 600 seconds = 10 mins
        );

        return (int)result == 1;
    }
    
    public async Task ReleaseLocksAsync(Guid showtimeId, Guid[] seatIds)
    {
        var db = _valKeyStore.GetDatabase();
        var keys = seatIds.Select(id => (RedisKey)$"lock:{showtimeId}:{id}").ToArray();
        await db.KeyDeleteAsync(keys);
    }
}