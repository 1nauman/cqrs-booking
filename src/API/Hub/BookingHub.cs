namespace Booking.API.Hub;

public class BookingHub : Microsoft.AspNetCore.SignalR.Hub
{
    // Frontend call this: connection.invoke("JoinShowtime", "showtime_guid")
    public async Task JoinShowtime(string showtimeId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, showtimeId);
    // Add this connection to a specific group so they only get updates for this event/movie
}