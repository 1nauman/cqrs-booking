namespace Booking.Domain.Repositories;

public interface IBookingRepository
{
    Task AddAsync(Entities.Booking booking, CancellationToken ct);
    Task<Entities.Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct);
    Task UpdateAsync(Entities.Booking booking, CancellationToken ct);
}