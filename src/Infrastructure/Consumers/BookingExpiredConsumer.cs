using Booking.Application.Abstractions;
using Booking.Domain.Events;
using Booking.Infrastructure.ReadModels;

using MassTransit;

using MongoDB.Driver;

namespace Booking.Infrastructure.Consumers;

public class BookingExpiredConsumer : IConsumer<BookingExpiredEvent>
{
    private readonly IMongoCollection<ShowtimeDocument> _mongoCollection;
    private readonly IRealTimeNotifier _notifier;

    public BookingExpiredConsumer(IMongoDatabase mongoDatabase, IRealTimeNotifier notifier)
    {
        _mongoCollection = mongoDatabase.GetCollection<ShowtimeDocument>("showtimes");
        _notifier = notifier;
    }

    public async Task Consume(ConsumeContext<BookingExpiredEvent> context)
    {
        var message = context.Message;

        // 1. Update MongoDB (Read Model)
        // We need to flip all these seats back to "Available" efficiently.
        // We use BulkWrite to send multiple updates to Mongo in one network call.
        var bulkOps = new List<WriteModel<ShowtimeDocument>>();

        foreach (var seatId in message.SeatIds)
        {
            // Filter: Match the Showtime AND the specific Seat inside the nested array
            var filter = Builders<ShowtimeDocument>.Filter.Eq(s => s.Id, message.ShowtimeId) &
                         Builders<ShowtimeDocument>.Filter.ElemMatch(s => s.Seats, s => s.SeatId == seatId);

            // Update: Reset status to "Available" and clear the ReserverId
            var update = Builders<ShowtimeDocument>.Update
                .Set("Seats.$.Status", "Available")
                .Set("Seats.$.ReserverId", (Guid?)null);

            bulkOps.Add(new UpdateOneModel<ShowtimeDocument>(filter, update));
        }

        if (bulkOps.Any())
        {
            await _mongoCollection.BulkWriteAsync(bulkOps);
        }

        // 2. Notify Frontend (SignalR)
        // We use the generic "Status Change" method we defined earlier.
        // Status: "Available" -> UI turns seat White/Clickable.
        // UserId: null -> No one owns it.
        await _notifier.NotifySeatsStatusChangeAsync(
            message.ShowtimeId,
            message.SeatIds,
            "Available",
            null 
        );

        Console.WriteLine($"[Consumer] Expired booking {message.BookingId}. Released {message.SeatIds.Length} seats.");
    }
}