using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Contracts.Events;

/// <summary>
/// Published when a booking is confirmed (after payment/approval — before container allocation).
/// Consumers: Container Service (trigger allocation), Notification Service.
/// </summary>
/// <param name="BookingId">Unique identifier of the booking.</param>
/// <param name="BookingNumber">Human-readable booking reference.</param>
/// <param name="CustomerId">The customer who owns this booking.</param>
/// <param name="Status">The new booking status (always CONFIRMED at time of this event).</param>
/// <param name="TimestampUtc">When this event was produced.</param>
/// <param name="CorrelationId">Tracing identifier shared across the request chain.</param>
public sealed record BookingConfirmed(
    Guid BookingId,
    string BookingNumber,
    Guid CustomerId,
    BookingStatus Status,
    DateTime TimestampUtc,
    Guid CorrelationId);