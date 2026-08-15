using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Container.Api.Domain;

/// <summary>
/// A physical shipping container. Status transitions are enforced by
/// <see cref="TransitionTo"/>; invalid jumps (e.g. AVAILABLE → DELIVERED)
/// throw <see cref="InvalidContainerTransitionException"/>.
/// </summary>
public sealed class Container
{
    public Guid Id { get; set; }
    public string ContainerNumber { get; set; } = default!;
    public ContainerType Type { get; set; }
    public ContainerStatus Status { get; set; }
    public int Teu { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Linear operational lifecycle, with the container returned to the pool on delivery:
    /// AVAILABLE → ALLOCATED → IN_YARD → LOADED → IN_TRANSIT → DISCHARGED → DELIVERED → AVAILABLE
    /// Side edges: AVAILABLE → IN_YARD (yard positioning), ALLOCATED → AVAILABLE (release),
    /// and ALLOCATED → IN_YARD is NOT allowed (must stay allocated until loaded).
    /// </summary>
    private static readonly IReadOnlyDictionary<ContainerStatus, ContainerStatus[]> AllowedTransitions =
        new Dictionary<ContainerStatus, ContainerStatus[]>
        {
            [ContainerStatus.AVAILABLE] = [ContainerStatus.ALLOCATED, ContainerStatus.IN_YARD],
            [ContainerStatus.ALLOCATED] = [ContainerStatus.IN_YARD, ContainerStatus.AVAILABLE],
            [ContainerStatus.IN_YARD] = [ContainerStatus.LOADED],
            [ContainerStatus.LOADED] = [ContainerStatus.IN_TRANSIT],
            [ContainerStatus.IN_TRANSIT] = [ContainerStatus.DISCHARGED],
            [ContainerStatus.DISCHARGED] = [ContainerStatus.DELIVERED],
            [ContainerStatus.DELIVERED] = [ContainerStatus.AVAILABLE],
        };

    public void TransitionTo(ContainerStatus newStatus)
    {
        if (Status == newStatus)
            throw new InvalidContainerTransitionException(
                $"Container {ContainerNumber} is already {Status}.");

        if (!AllowedTransitions[Status].Contains(newStatus))
            throw new InvalidContainerTransitionException(
                $"Cannot transition container {ContainerNumber} from {Status} to {newStatus}.");

        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}