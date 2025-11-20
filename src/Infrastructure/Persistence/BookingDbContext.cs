using Booking.Domain.Entities;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence;

public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Showtime> Showtimes => Set<Showtime>();

    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<Domain.Entities.Booking> Bookings => Set<Domain.Entities.Booking>();

    public DbSet<BookingItem> BookingItems => Set<BookingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configure MassTransit Outbox Tables
        // This creates tables: OutboxMessage, OutboxState, InboxState
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // 2. Configure Seat Concurrency
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Id);

            // This is the magic line for Optimistic Concurrency in Postgres
            // If the version in DB != version in memory during save, it throws DbUpdateConcurrencyException
            entity.Property(e => e.Version).IsConcurrencyToken();
        });

        // 3. Configure Relationships
        modelBuilder.Entity<Showtime>()
            .HasMany(s => s.Seats)
            .WithOne()
            .HasForeignKey(s => s.ShowtimeId);

        // Configure Booking Aggregate
        modelBuilder.Entity<Domain.Entities.Booking>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configure the One-to-many relationship
            entity.HasMany(b => b.Items)
                .WithOne()
                .HasForeignKey(k => k.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingItem>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}