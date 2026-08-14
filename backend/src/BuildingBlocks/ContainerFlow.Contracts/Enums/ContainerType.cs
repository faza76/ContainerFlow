namespace ContainerFlow.Contracts.Enums;

/// <summary>
/// Physical container size/type. Part of the cross-service public contract.
/// </summary>
public enum ContainerType
{
    /// <summary>20-foot standard container.</summary>
    FT20 = 0,

    /// <summary>40-foot standard container.</summary>
    FT40 = 1,

    /// <summary>40-foot high-cube container.</summary>
    FT40HC = 2,

    /// <summary>Reefer (temperature-controlled) container.</summary>
    REEFER = 3
}
