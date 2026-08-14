namespace ContainerFlow.Contracts.Enums;

/// <summary>
/// Lifecycle of a booking/shipment as tracked by the Booking Service.
/// These values are part of the cross-service public contract — do not
/// rename or reorder without coordinating with every consumer.
/// </summary>
public enum BookingStatus
{
    PENDING = 0,
    CONFIRMED = 1,
    CANCELLED = 2,
    IN_PROGRESS = 3,
    COMPLETED = 4
}
