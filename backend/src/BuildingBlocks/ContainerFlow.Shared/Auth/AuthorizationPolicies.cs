namespace ContainerFlow.Shared.Auth;

/// <summary>
/// Pure helper functions for role-based authorization checks.
/// These encode the authorization table as re-usable predicates
/// without enforcing policy — each service still calls its own
/// [Authorize] attributes or middleware.
/// </summary>
public static class AuthorizationPolicies
{
    public static bool CanManageBookings(string role)
        => role is AuthConstants.RoleAdmin or AuthConstants.RoleStaff;

    public static bool CanCreateBookings(string role)
        => role is AuthConstants.RoleAdmin or AuthConstants.RoleStaff or AuthConstants.RoleCustomer;

    public static bool CanViewOwnBookings(string role)
        => role is AuthConstants.RoleAdmin or AuthConstants.RoleStaff or AuthConstants.RoleCustomer;

    public static bool CanManageContainers(string role)
        => role is AuthConstants.RoleAdmin or AuthConstants.RoleStaff;

    public static bool CanViewNotifications(string role)
        => role is AuthConstants.RoleAdmin or AuthConstants.RoleStaff or AuthConstants.RoleCustomer;

    public static bool CanManageNotificationUsers(string role)
        => role is AuthConstants.RoleAdmin;
}