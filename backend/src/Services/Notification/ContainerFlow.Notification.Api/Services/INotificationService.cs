using ContainerFlow.Notification.Api.Domain;

namespace ContainerFlow.Notification.Api.Services;

public interface INotificationService
{
    /// <summary>
    /// Creates a notification unless one with the same <paramref name="idempotencyKey"/>
    /// already exists (RabbitMQ can redeliver messages). Safe to call twice for the
    /// same event — returns the existing row on duplicate.
    /// </summary>
    Task<Domain.Notification?> CreateIfNewAsync(
        string idempotencyKey,
        string eventType,
        Guid? customerId,
        Guid? bookingId,
        Guid? containerId,
        string title,
        string message,
        CancellationToken ct = default);

    Task<Domain.Notification?> GetAsync(Guid id, Guid? customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Notification>> GetAllAsync(Guid? customerId, CancellationToken ct = default);
}