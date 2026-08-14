using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Contracts.Events;

/// <summary>
/// Published when containers are physically allocated to a booking.
/// Consumers: Booking Service (update booking status), Notification Service.
/// </summary>
/// <param name="BookingId">The booking the containers are allocated to.</param>
/// <param name="BookingNumber">Human-readable booking reference.</param>
/// <param name="ContainerId">Unique identifier of the allocated container.</param>
/// <param name="ContainerNumber">Human-readable container ID (e.g. "MSCU1234567").</param>
/// <param name="ContainerType">Type of the allocated container.</param>
/// <param name="TimestampUtc">When this event was produced.</param>
/// <param name="CorrelationId">Tracing identifier shared across the request chain.</param>
public sealed record ContainerAllocated(
    Guid BookingId,
    string BookingNumber,
    Guid ContainerId,
    string ContainerNumber,
    ContainerType ContainerType,
    DateTime TimestampUtc,
    Guid CorrelationId);