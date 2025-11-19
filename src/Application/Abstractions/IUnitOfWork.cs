namespace Booking.Application.Abstractions;

public interface IUnitOfWork
{
    // Commits changes to Postgres (including the Outbox messages)
    Task SaveChangesAsync(CancellationToken ct);
}