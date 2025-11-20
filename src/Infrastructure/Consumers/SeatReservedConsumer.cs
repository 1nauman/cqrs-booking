using Booking.Application.Abstractions;
using Booking.Domain.Events;
using Booking.Infrastructure.ReadModels;

using MassTransit;

using MongoDB.Driver;

namespace Booking.Infrastructure.Consumers;

public class SeatsReservedConsumer : IConsumer<SeatsReservedEvent>
{
    private readonly IMongoCollection<ShowtimeDocument> _mongoCollection;
    private readonly IRealTimeNotifier _notifier;

    public SeatsReservedConsumer(IMongoDatabase mongoDatabase, IRealTimeNotifier notifier)
    {
        _mongoCollection = mongoDatabase.GetCollection<ShowtimeDocument>("showtimes");
        _notifier = notifier;
    }

    public async Task Consume(ConsumeContext<SeatsReservedEvent> context)
    {
        var message = context.Message;

        // 1. Bulk Update MongoDB
        // We want to update multiple array elements in the SAME document.
        // The most efficient way in Mongo driver is a BulkWrite.
        var bulkOps = new List<WriteModel<ShowtimeDocument>>();

        foreach (var seat in message.Seats)
        {
            // Filter: Find the Showtime AND the specific Seat inside it
            var filter = Builders<ShowtimeDocument>.Filter.Eq(s => s.Id, message.ShowtimeId) &
                         Builders<ShowtimeDocument>.Filter.ElemMatch(s => s.Seats, s => s.SeatId == seat.SeatId);

            // Update: Set status to Reserved
            var update = Builders<ShowtimeDocument>.Update
                .Set("Seats.$.Status", "Reserved")
                .Set("Seats.$.ReserverId", message.UserId);

            bulkOps.Add(new UpdateOneModel<ShowtimeDocument>(filter, update));
        }

        if (bulkOps.Any())
        {
            await _mongoCollection.BulkWriteAsync(bulkOps);
        }

        // 2. Batch Notify SignalR (One message payload)
        var seatIds = message.Seats.Select(s => s.SeatId).ToArray();
        await _notifier.NotifySeatsLockedAsync(message.ShowtimeId, seatIds, message.UserId);

        Console.WriteLine($"[Consumer] Batch reserved {message.Seats.Count} seats.");
    }
}