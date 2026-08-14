using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ContainerFlow.Shared.Correlation;

/// <summary>
/// Extension methods for registering the correlation ID middleware
/// and retrieving the correlation ID from <see cref="HttpContext"/>.
///
/// <code>
/// // Program.cs
/// app.UseCorrelationId();
///
/// // In a controller or service:
/// var correlationId = HttpContext.GetCorrelationId();
/// </code>
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// Adds the <see cref="CorrelationIdMiddleware"/> to the pipeline.
    /// Call this early (before <c>UseRouting</c> / <c>UseAuthorization</c>).
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(
        this IApplicationBuilder app,
        string headerName = CorrelationIdMiddleware.DefaultHeaderName)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>(headerName);
    }

    /// <summary>
    /// Retrieves the correlation ID stored by <see cref="CorrelationIdMiddleware"/>
    /// from the current <see cref="HttpContext.Items"/>.
    /// Returns <c>null</c> if the middleware hasn't run yet.
    /// </summary>
    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var value)
            ? value as string
            : null;
    }
}