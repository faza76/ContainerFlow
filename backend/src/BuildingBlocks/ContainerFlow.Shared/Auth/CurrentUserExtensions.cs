using Microsoft.Extensions.DependencyInjection;

namespace ContainerFlow.Shared.Auth;

/// <summary>
/// Registers <see cref="ICurrentUser"/> (and its <c>IHttpContextAccessor</c> dependency)
/// in the DI container. Call once per service in <c>Program.cs</c>.
/// </summary>
public static class CurrentUserExtensions
{
    public static IServiceCollection AddContainerFlowCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}