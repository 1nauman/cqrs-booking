namespace Booking.Domain.Entities;

public class Showtime
{
    public Guid Id { get; private set; }
    public string MovieTitle { get; private set; } = string.Empty;
    public DateTime StartTime { get; private set; }

    // Navigation Property
    public ICollection<Seat> Seats { get; private set; } = new List<Seat>();

    // Constructor for EF Core
    private Showtime() { }

    public Showtime(Guid id, string movieTitle, DateTime startTime)
    {
        Id = id;
        MovieTitle = movieTitle;
        StartTime = startTime;
    }

}