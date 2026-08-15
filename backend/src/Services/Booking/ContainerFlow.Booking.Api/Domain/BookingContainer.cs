namespace ContainerFlow.Booking.Api.Domain;

/// <summary>
/// A container reference on a booking. The Booking Service stores only the
/// container number/id — it never queries container_db directly.
/// </summary>
public sealed class BookingContainer
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid? ContainerId { get; set; }
    public string ContainerNumber { get; set; } = default!;
}