using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ContainerFlow.Shared.Auth;

/// <summary>
/// Resolves the authenticated user for the current request from JWT claims.
/// Registered via <see cref="CurrentUserExtensions.AddContainerFlowCurrentUser"/>.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? Id { get; }
    string? Role { get; }
    Guid? CustomerId { get; }
    bool IsInRole(string role);
}

/// <summary>
/// Reads the current user from <see cref="System.Security.Claims.ClaimsPrincipal"/>
/// using the claim constants in <see cref="AuthConstants"/>.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid? Id =>
        Guid.TryParse(Principal?.FindFirst(AuthConstants.ClaimSub)?.Value, out var id) ? id : null;

    public string? Role => Principal?.FindFirst(AuthConstants.ClaimRole)?.Value;

    public Guid? CustomerId =>
        Guid.TryParse(Principal?.FindFirst(AuthConstants.ClaimCustomerId)?.Value, out var cid)
            ? cid
            : null;

    public bool IsInRole(string role) =>
        string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
}