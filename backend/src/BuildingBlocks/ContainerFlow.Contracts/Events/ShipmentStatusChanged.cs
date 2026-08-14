using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Contracts.Events;

/// <summary>
/// Published when a booking/shipment status changes to a new lifecycle stage.
/// Consumers: Notification Service, potentially Gateway for SSE push.
/// </summary>
/// <param name="BookingId">The booking whose status changed.</param>
/// <param name="BookingNumber">Human-readable booking reference.</param>
/// <param name="ContainerId">Optional — the container driving this status change, if applicable.</param>
/// <param name="ContainerNumber">Optional — human-readable container ID.</param>
/// <param name="OldStatus">Previous booking status.</param>
/// <param name="NewStatus">Current booking status after the change.</param>
/// <param name="TimestampUtc">When this event was produced.</param>
/// <param name="CorrelationId">Tracing identifier shared across the request chain.</param>
public sealed record ShipmentStatusChanged(
    Guid BookingId,
    string BookingNumber,
    Guid? ContainerId,
    string? ContainerNumber,
    BookingStatus OldStatus,
    BookingStatus NewStatus,
    DateTime TimestampUtc,
    Guid CorrelationId);