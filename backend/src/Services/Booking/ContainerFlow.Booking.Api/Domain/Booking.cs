using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Booking.Api.Domain;

/// <summary>
/// A booking (commercial/operational) — the Booking Service's aggregate root.
/// Status transitions are enforced by <see cref="TransitionTo"/>; invalid
/// transitions throw <see cref="InvalidBookingTransitionException"/>.
/// </summary>
public sealed class Booking
{
    public Guid Id { get; set; }
    public string BookingNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string Origin { get; set; } = default!;
    public string Destination { get; set; } = default!;
    public string Cargo { get; set; } = default!;
    public ContainerType ContainerType { get; set; }
    public int ContainerCount { get; set; }
    public string? Vessel { get; set; }
    public string? Voyage { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>References to allocated containers (no FK into container_db).</summary>
    public ICollection<BookingContainer> Containers { get; set; } = new List<BookingContainer>();

    /// <summary>
    /// Valid lifecycle: PENDING → CONFIRMED → IN_PROGRESS → COMPLETED, plus CANCELLED.
    /// </summary>
    private static readonly IReadOnlyDictionary<BookingStatus, BookingStatus[]> AllowedTransitions =
        new Dictionary<BookingStatus, BookingStatus[]>
        {
            [BookingStatus.PENDING] = [BookingStatus.CONFIRMED, BookingStatus.CANCELLED],
            [BookingStatus.CONFIRMED] = [BookingStatus.IN_PROGRESS, BookingStatus.CANCELLED],
            [BookingStatus.IN_PROGRESS] = [BookingStatus.COMPLETED, BookingStatus.CANCELLED],
            [BookingStatus.CANCELLED] = [],
            [BookingStatus.COMPLETED] = [],
        };

    public void TransitionTo(BookingStatus newStatus)
    {
        if (Status == newStatus)
            throw new InvalidBookingTransitionException(
                $"Booking {BookingNumber} is already {Status}.");

        if (!AllowedTransitions[Status].Contains(newStatus))
            throw new InvalidBookingTransitionException(
                $"Cannot transition booking {BookingNumber} from {Status} to {newStatus}.");

        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}