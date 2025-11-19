using Booking.Domain.Repositories;
using Booking.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task AddAsync(Domain.Entities.Booking booking, CancellationToken ct)
    {
        await _context.Bookings.AddAsync(booking, ct);
    }

    public async Task<Domain.Entities.Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct)
    {
        return await _context.Bookings.FirstOrDefaultAsync(o => o.Id == bookingId, ct);
    }

    public Task UpdateAsync(Domain.Entities.Booking booking, CancellationToken ct)
    {
        _context.Bookings.Update(booking);
        return Task.CompletedTask;
    }
}