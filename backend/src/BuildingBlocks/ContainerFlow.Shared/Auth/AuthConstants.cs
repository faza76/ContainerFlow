namespace ContainerFlow.Shared.Auth;

/// <summary>
/// Constants for JWT claim types and shared role values.
/// All services and the Gateway should reference these constants
/// rather than hardcoding claim/role strings.
/// </summary>
public static class AuthConstants
{
    // ── JWT claim types ──────────────────────────────────────────
    /// <summary>Subject — the user's unique identifier (Guid).</summary>
    public const string ClaimSub = "sub";

    /// <summary>User's display name.</summary>
    public const string ClaimName = "name";

    /// <summary>User's role (admin, staff, customer).</summary>
    public const string ClaimRole = "role";

    /// <summary>Customer identifier — present only for customer-role tokens.</summary>
    public const string ClaimCustomerId = "customer_id";

    // ── Role values (mirrors ContainerFlow.Contracts.Enums.Role) ──
    public const string RoleAdmin = "admin";
    public const string RoleStaff = "staff";
    public const string RoleCustomer = "customer";
}