using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Contracts.Events;

/// <summary>
/// Published when a new booking is created in the Booking Service.
/// Consumers: Notification Service (send confirmation), Container Service (await allocation).
/// </summary>
/// <param name="BookingId">Unique identifier of the booking.</param>
/// <param name="BookingNumber">Human-readable booking reference (e.g. "BK-2026-00042").</param>
/// <param name="CustomerId">The customer who owns this booking.</param>
/// <param name="CustomerName">Display name of the customer.</param>
/// <param name="ContainerType">Type of container requested.</param>
/// <param name="ContainerCount">Number of containers requested.</param>
/// <param name="TimestampUtc">When this event was produced.</param>
/// <param name="CorrelationId">Tracing identifier shared across the request chain.</param>
public sealed record BookingCreated(
    Guid BookingId,
    string BookingNumber,
    Guid CustomerId,
    string CustomerName,
    ContainerType ContainerType,
    int ContainerCount,
    DateTime TimestampUtc,
    Guid CorrelationId);