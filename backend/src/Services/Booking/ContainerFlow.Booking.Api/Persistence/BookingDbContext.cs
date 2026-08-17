using ContainerFlow.Booking.Api.Domain;
using ContainerFlow.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace ContainerFlow.Booking.Api.Persistence;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options)
    : DbContext(options)
{
    public DbSet<Domain.Booking> Bookings => Set<Domain.Booking>();
    public DbSet<BookingContainer> BookingContainers => Set<BookingContainer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Booking>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.BookingNumber).HasMaxLength(32).IsRequired();
            b.HasIndex(x => x.BookingNumber).IsUnique();

            b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.CustomerId); // customer-scoped queries

            b.Property(x => x.Origin).HasMaxLength(200).IsRequired();
            b.Property(x => x.Destination).HasMaxLength(200).IsRequired();
            b.Property(x => x.Cargo).HasMaxLength(500).IsRequired();
            b.Property(x => x.Vessel).HasMaxLength(100);
            b.Property(x => x.Voyage).HasMaxLength(100);

            // Enums stored as readable strings (not magic ints) in PostgreSQL.
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            b.Property(x => x.ContainerType).HasConversion<string>().HasMaxLength(16);

            b.HasMany(x => x.Containers)
                .WithOne()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingContainer>(c =>
        {
            c.HasKey(x => x.Id);
            c.Property(x => x.ContainerNumber).HasMaxLength(16).IsRequired();
        });
    }
}