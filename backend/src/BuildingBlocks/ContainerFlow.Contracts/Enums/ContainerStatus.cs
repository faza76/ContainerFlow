namespace ContainerFlow.Contracts.Enums;

/// <summary>
/// Operational state of a container as tracked by the Container Service.
/// Part of the cross-service public contract.
/// </summary>
public enum ContainerStatus
{
    AVAILABLE = 0,
    ALLOCATED = 1,
    IN_YARD = 2,
    LOADED = 3,
    IN_TRANSIT = 4,
    DISCHARGED = 5,
    DELIVERED = 6
}
