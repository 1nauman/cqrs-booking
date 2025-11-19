using Booking.Domain.Entities;

namespace Booking.Domain.Repositories;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(Guid seatId, CancellationToken ct);
    Task<Seat?> GetByPositionAsync(Guid showtimeId, string row, int number, CancellationToken ct);
    Task UpdateAsync(Seat seat, CancellationToken ct);
}