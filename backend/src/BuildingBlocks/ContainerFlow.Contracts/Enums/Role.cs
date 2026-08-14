namespace ContainerFlow.Contracts.Enums;

/// <summary>
/// User roles used across services (JWT role claim values).
/// Part of the cross-service public contract.
/// </summary>
public enum Role
{
    admin = 0,
    staff = 1,
    customer = 2
}
