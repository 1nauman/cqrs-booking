namespace Booking.Application.Abstractions;

public interface IRealTimeNotifier
{
    // "Notify that these seats have changed status"
    // No "Batch", no "Update", just the domain fact.
    Task NotifySeatsStatusChangeAsync(Guid showtimeId, Guid[] seatIds, string newStatus, Guid? userId);
}