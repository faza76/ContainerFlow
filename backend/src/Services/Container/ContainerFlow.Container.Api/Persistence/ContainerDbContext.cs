using ContainerFlow.Container.Api.Domain;
using ContainerFlow.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace ContainerFlow.Container.Api.Persistence;

public sealed class ContainerDbContext(DbContextOptions<ContainerDbContext> options)
    : DbContext(options)
{
    public DbSet<Domain.Container> Containers => Set<Domain.Container>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Container>(c =>
        {
            c.HasKey(x => x.Id);
            c.Property(x => x.ContainerNumber).HasMaxLength(16).IsRequired();
            c.HasIndex(x => x.ContainerNumber).IsUnique();

            c.Property(x => x.Location).HasMaxLength(200);
            c.Property(x => x.BookingNumber).HasMaxLength(32);

            // Enums stored as readable strings.
            c.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            c.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);

            c.HasIndex(x => x.Status);
            c.HasIndex(x => x.BookingId);
        });
    }
}