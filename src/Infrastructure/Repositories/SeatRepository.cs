using Booking.Domain.Entities;
using Booking.Domain.Repositories;
using Booking.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly BookingDbContext _context;

    public SeatRepository(BookingDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<Seat?> GetByIdAsync(Guid seatId, CancellationToken ct)
    {
        return await _context.Seats.FirstOrDefaultAsync(o => o.Id == seatId, ct);
    }
    
    public async Task<List<Seat>> GetByIdsAsync(Guid showtimeId, Guid[] seatIds, CancellationToken ct)
    {
        // We filter by BOTH SeatIds AND ShowtimeId.
        // This prevents a user from sending a valid SeatId that belongs to a DIFFERENT showtime.
        return await _context.Seats
            .Where(s => seatIds.Contains(s.Id) && s.ShowtimeId == showtimeId)
            .ToListAsync(ct);
    }

    public async Task<Seat?> GetByPositionAsync(Guid showtimeId, string row, int number, CancellationToken ct)
    {
        return await _context.Seats.FirstOrDefaultAsync(
            o => o.ShowtimeId == showtimeId && o.Row == row && o.Number == number, ct);
    }

    public Task UpdateAsync(Seat seat, CancellationToken ct)
    {
        _context.Seats.Update(seat);
        return Task.CompletedTask;
    }
}