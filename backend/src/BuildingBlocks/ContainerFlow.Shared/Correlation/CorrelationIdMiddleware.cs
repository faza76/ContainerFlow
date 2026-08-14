using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ContainerFlow.Shared.Correlation;

/// <summary>
/// Middleware that reads or generates a correlation ID for every request,
/// places it into <see cref="HttpContext.Items"/> and the logging scope.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    internal const string DefaultHeaderName = "X-Correlation-Id";
    internal const string ItemsKey = "ContainerFlow.CorrelationId";

    private readonly RequestDelegate _next;
    private readonly string _headerName;

    public CorrelationIdMiddleware(RequestDelegate next, string headerName = DefaultHeaderName)
    {
        _next = next;
        _headerName = headerName;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        // Read from incoming request header, or generate a new one.
        var correlationId = context.Request.Headers.TryGetValue(_headerName, out var values)
            && values.FirstOrDefault() is { } val && Guid.TryParse(val, out var parsed)
                ? parsed
                : Guid.NewGuid();

        var correlationIdStr = correlationId.ToString("D");

        // Store in Items so downstream middleware and handlers can retrieve it.
        context.Items[ItemsKey] = correlationIdStr;

        // Set the response header so the caller can correlate.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[_headerName] = correlationIdStr;
            return Task.CompletedTask;
        });

        // Add to the logging scope so every log line during this request carries it.
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationIdStr }))
        {
            await _next(context);
        }
    }
}