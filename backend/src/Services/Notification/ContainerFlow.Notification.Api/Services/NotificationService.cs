using ContainerFlow.Notification.Api.Domain;
using ContainerFlow.Notification.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContainerFlow.Notification.Api.Services;

public sealed class NotificationService(
    NotificationDbContext db,
    ILogger<NotificationService> logger)
    : INotificationService
{
    public async Task<Domain.Notification?> CreateIfNewAsync(
        string idempotencyKey,
        string eventType,
        Guid? customerId,
        Guid? bookingId,
        Guid? containerId,
        string title,
        string message,
        CancellationToken ct = default)
    {
        var existing = await db.Notifications.FirstOrDefaultAsync(n => n.IdempotencyKey == idempotencyKey, ct);
        if (existing is not null)
            return existing;

        var notification = new Domain.Notification
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            BookingId = bookingId,
            ContainerId = containerId,
            EventType = eventType,
            Title = title,
            Message = message,
            IdempotencyKey = idempotencyKey,
            CreatedAtUtc = DateTime.UtcNow,
            IsRead = false
        };

        db.Notifications.Add(notification);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Unique index on IdempotencyKey: a concurrent redelivery inserted first.
            logger.LogInformation("Duplicate notification for event {EventType} ignored ({IdempotencyKey})",
                eventType, idempotencyKey);
            return await db.Notifications.FirstOrDefaultAsync(n => n.IdempotencyKey == idempotencyKey, ct);
        }

        logger.LogInformation("Notification {NotificationId} created from event {EventType}",
            notification.Id, eventType);
        return notification;
    }

    public async Task<Domain.Notification?> GetAsync(Guid id, Guid? customerId, CancellationToken ct = default)
    {
        var query = db.Notifications.AsQueryable();
        if (customerId is not null)
            query = query.Where(n => n.CustomerId == customerId);

        return await query.FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<IReadOnlyList<Domain.Notification>> GetAllAsync(Guid? customerId, CancellationToken ct = default)
    {
        var query = db.Notifications.AsQueryable();
        if (customerId is not null)
            query = query.Where(n => n.CustomerId == customerId);

        return await query.OrderByDescending(n => n.CreatedAtUtc).ToListAsync(ct);
    }
}