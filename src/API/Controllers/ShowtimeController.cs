using Booking.Infrastructure.ReadModels;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Driver;

namespace Booking.API.Controllers;

[ApiController]
[Route("api/showtimes")]
public class ShowtimeController : ControllerBase
{
    private readonly IMongoCollection<ShowtimeDocument> _collection;

    public ShowtimeController(IMongoDatabase db)
    {
        _collection = db.GetCollection<ShowtimeDocument>("showtimes");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var showtime = await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (showtime == null) return NotFound();
        return Ok(showtime);
    }
}