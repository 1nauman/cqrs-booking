using Booking.Application.Abstractions;
using Booking.Application.Commands;

using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly ICommandHandler<ReserveSeatsCommand, bool> _reserveSeatHandler;

    public BookingController(ICommandHandler<ReserveSeatsCommand, bool> reserveSeatHandler)
    {
        ArgumentNullException.ThrowIfNull(reserveSeatHandler);
        _reserveSeatHandler = reserveSeatHandler;
    }

    [HttpPost]
    public async Task<IActionResult> ReserveSeat([FromBody] ReserveSeatRequest request)
    {
        var command = new ReserveSeatsCommand(request.ShowtimeId, request.SeatIds, request.UserId);
        var success = await _reserveSeatHandler.HandleAsync(command, HttpContext.RequestAborted);

        if (!success)
        {
            // 409 Conflict: "Someone else got it first" or "Already sold"
            return Conflict(new { message = "Unavailable seat." });
        }

        return Ok(new { message = "Seat reserved successfully." });
    }
}

public record ReserveSeatRequest(Guid ShowtimeId, Guid[] SeatIds, Guid UserId);