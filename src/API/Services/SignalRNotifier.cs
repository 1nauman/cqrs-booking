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

    public async Task NotifySeatsStatusChangeAsync(Guid showtimeId, Guid[] seatIds, string newStatus, Guid? userId)
    {
        // The Client Method Name is now "SeatsStatusChanged"
        await _hubContext.Clients.Group(showtimeId.ToString())
            .SendAsync("SeatsStatusChanged", new 
            { 
                SeatIds = seatIds, 
                Status = newStatus, 
                UserId = userId 
            });
    }
}