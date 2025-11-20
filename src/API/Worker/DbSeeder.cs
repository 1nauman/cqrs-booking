using Booking.Domain.Entities;
using Booking.Infrastructure.Persistence;
using Booking.Infrastructure.ReadModels;

using Microsoft.EntityFrameworkCore;

using MongoDB.Bson;
using MongoDB.Driver;

namespace Booking.API.Worker;

public static class DbSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var mongoDb = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var mongoCollection = mongoDb.GetCollection<ShowtimeDocument>("showtimes");

        // Ensure DB is created
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Postgres
        if (!context.Showtimes.Any())
        {
            var showtimeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var showtime = new Showtime(showtimeId, "Bourne Identity", DateTime.UtcNow.AddHours(2));

            context.Showtimes.Add(showtime);

            var rows = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N" };
            foreach (string row in rows)
            {
                for (int i = 1; i <= 10; i++)
                {
                    context.Seats.Add(new Seat(showtimeId, row, i));
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Database seeded with Bourne Identity and 140 seats");
        }

        // 2. Seed Mongo (Sync from Postgres)
        var count = await mongoCollection.CountDocumentsAsync(new BsonDocument());
        if (count == 0)
        {
            var showtime = await context.Showtimes
                .Include(o => o.Seats)
                .FirstOrDefaultAsync();

            if (showtime != null)
            {
                var doc = new ShowtimeDocument
                {
                    Id = showtime.Id,
                    MovieTitle = showtime.MovieTitle,
                    StartTime = showtime.StartTime,
                    Seats = showtime.Seats.Select(s => new SeatDocument
                    {
                        SeatId = s.Id,
                        Row = s.Row,
                        Number = s.Number,
                        Status = s.Status.ToString()
                    }).ToList()
                };

                await mongoCollection.InsertOneAsync(doc);
                Console.WriteLine("✅ MongoDB seeded.");
            }
        }
    }
}