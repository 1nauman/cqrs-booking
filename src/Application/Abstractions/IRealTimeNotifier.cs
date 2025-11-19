namespace Booking.Application.Abstractions;

public interface IRealTimeNotifier
{
    Task NotifySeatLockedAsync(Guid showtimeId, Guid seatId, Guid userId);
}