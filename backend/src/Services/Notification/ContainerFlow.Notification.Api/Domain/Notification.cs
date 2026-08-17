namespace ContainerFlow.Notification.Api.Domain;

/// <summary>
/// A human-readable notification created by consuming events.
/// Created only by MassTransit consumers — never via a client POST.
/// </summary>
public sealed class Notification
{
    public Guid Id { get; set; }

    /// <summary>Customer this notification belongs to (for customer-scoped views).</summary>
    public Guid? CustomerId { get; set; }

    public Guid? BookingId { get; set; }
    public Guid? ContainerId { get; set; }

    /// <summary>Name of the source event, e.g. "BookingConfirmed".</summary>
    public string EventType { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;

    /// <summary>
    /// Composite key (event type + source entity id + timestamp) that makes
    /// duplicate deliveries of the same event idempotent.
    /// </summary>
    public string IdempotencyKey { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }
}