using Booking.Application.Abstractions;
using Booking.Infrastructure.Persistence;

namespace Booking.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BookingDbContext _context;

    public UnitOfWork(BookingDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}