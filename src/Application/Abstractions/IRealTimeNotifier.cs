namespace Booking.Application.Abstractions;

public interface IRealTimeNotifier
{
    // Old singular method (can keep or remove)
    Task NotifySeatLockedAsync(Guid showtimeId, Guid seatId, Guid userId);

    // New Batch method
    Task NotifySeatsLockedAsync(Guid showtimeId, Guid[] seatIds, Guid userId);
}