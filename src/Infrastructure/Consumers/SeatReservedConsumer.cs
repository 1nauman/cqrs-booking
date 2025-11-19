using Booking.Application.Abstractions;
using Booking.Domain.Events;
using Booking.Infrastructure.ReadModels;

using MassTransit;

using MongoDB.Driver;

namespace Booking.Infrastructure.Consumers;

public class SeatReservedConsumer : IConsumer<SeatReservedEvent>
{
    private readonly IMongoCollection<ShowtimeDocument> _mongoCollection;
    private readonly IRealTimeNotifier _notifier;

    public SeatReservedConsumer(
        IMongoDatabase mongoDatabase,
        IRealTimeNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(mongoDatabase);
        ArgumentNullException.ThrowIfNull(notifier);

        _mongoCollection = mongoDatabase.GetCollection<ShowtimeDocument>("showtimes");
        _notifier = notifier;
    }

    public async Task Consume(ConsumeContext<SeatReservedEvent> context)
    {
        var message = context.Message;

        // 1. Update MongoDB (The Read Model)
        // We use an array filter to find the specific seat inside the document
        var filter = Builders<ShowtimeDocument>.Filter.Eq(s => s.Id, message.ShowtimeId) &
                     Builders<ShowtimeDocument>.Filter.ElemMatch(s => s.Seats, seat => seat.SeatId == message.SeatId);

        var update = Builders<ShowtimeDocument>.Update
            .Set("Seats.$.Status", "Reserved")
            .Set("Seats.$.ReserverId", message.UserId);

        await _mongoCollection.UpdateOneAsync(filter, update);

        // 2. Notify Frontend (SignalR)
        await _notifier.NotifySeatLockedAsync(message.ShowtimeId, message.SeatId, message.UserId);

        Console.WriteLine($"[Consumer] Seat {message.Row}{message.Number} marked as Reserved in Mongo & SignalR.");
    }
}