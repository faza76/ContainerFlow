using ContainerFlow.Notification.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContainerFlow.Notification.Api.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : DbContext(options)
{
    public DbSet<Domain.Notification> Notifications => Set<Domain.Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Notification>(n =>
        {
            n.HasKey(x => x.Id);
            n.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            n.Property(x => x.Title).HasMaxLength(200).IsRequired();
            n.Property(x => x.Message).HasMaxLength(1000).IsRequired();

            // The idempotency guard: unique key prevents duplicate notifications
            // for redelivered RabbitMQ messages.
            n.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
            n.HasIndex(x => x.IdempotencyKey).IsUnique();

            n.HasIndex(x => x.CustomerId);
            n.HasIndex(x => x.CreatedAtUtc);
        });
    }
}