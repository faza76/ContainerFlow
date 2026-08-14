using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ContainerFlow.Shared.Health;

/// <summary>
/// Extension methods for configuring the shared health-check infrastructure.
///
/// <code>
/// // In your service's Program.cs:
/// builder.Services.AddContainerFlowHealthChecks();
///     .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"));
/// </code>
///
/// Each service adds its own PostgreSQL (and RabbitMQ, for Notification Service)
/// checks on top of this base.
/// </summary>
public static class ContainerFlowHealthChecksExtensions
{
    /// <summary>
    /// Registers a minimal set of shared health checks (liveness, readiness)
    /// and returns the <see cref="IHealthChecksBuilder"/> for service-specific additions.
    /// </summary>
    public static IHealthChecksBuilder AddContainerFlowHealthChecks(
        this IServiceCollection services)
    {
        return services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("API is running"),
                tags: ["liveness"]);
    }
}