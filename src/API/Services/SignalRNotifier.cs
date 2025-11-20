using Booking.API.Hub;
using Booking.Application.Abstractions;

using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Services;

public class SignalRNotifier : IRealTimeNotifier
{
    private readonly IHubContext<BookingHub> _hubContext;

    public SignalRNotifier(IHubContext<BookingHub> hubContext)
    {
        ArgumentNullException.ThrowIfNull(hubContext);

        _hubContext = hubContext;
    }

    public async Task NotifySeatLockedAsync(Guid showtimeId, Guid seatId, Guid userId)
    {
        await _hubContext.Clients.Group(showtimeId.ToString())
            .SendAsync("ReceiveSeatUpdate", new
            {
                SeatId = seatId,
                UserId = userId,
                Status = "Reserved"
            });
    }

    public async Task NotifySeatsLockedAsync(Guid showtimeId, Guid[] seatIds, Guid userId)
    {
        // Send a single message with an ARRAY of IDs
        await _hubContext.Clients.Group(showtimeId.ToString())
            .SendAsync("ReceiveBatchSeatUpdate", new
            {
                SeatIds = seatIds,
                Status = "Reserved",
                UserId = userId
            });
    }
}