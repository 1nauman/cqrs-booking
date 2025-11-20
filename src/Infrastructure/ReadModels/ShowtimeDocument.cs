using MongoDB.Bson.Serialization.Attributes;

namespace Booking.Infrastructure.ReadModels;

public class ShowtimeDocument
{
    [BsonId]
    public Guid Id { get; set; }    // Matches Postgres ShowtimeId

    public string MovieTitle { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public List<SeatDocument> Seats { get; set; } = new();
}

public class SeatDocument
{
    public Guid SeatId { get; set; }

    public string Row { get; set; } = string.Empty;

    public int Number { get; set; }

    public string Status { get; set; } = "Available"; // Available, Reserved, Sold

    public Guid? ReserverId { get; set; }
}