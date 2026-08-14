using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Contracts.Events;

/// <summary>
/// Published when a container's operational status changes (e.g. IN_YARD → LOADED).
/// Consumers: Booking Service (may trigger ShipmentStatusChanged), Notification Service.
/// </summary>
/// <param name="ContainerId">Unique identifier of the container.</param>
/// <param name="ContainerNumber">Human-readable container ID (e.g. "MSCU1234567").</param>
/// <param name="OldStatus">Previous container status.</param>
/// <param name="NewStatus">Current container status after the change.</param>
/// <param name="TimestampUtc">When this event was produced.</param>
/// <param name="CorrelationId">Tracing identifier shared across the request chain.</param>
public sealed record ContainerStatusChanged(
    Guid ContainerId,
    string ContainerNumber,
    ContainerStatus OldStatus,
    ContainerStatus NewStatus,
    DateTime TimestampUtc,
    Guid CorrelationId);