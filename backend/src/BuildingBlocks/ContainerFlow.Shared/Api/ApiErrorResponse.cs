namespace ContainerFlow.Shared.Api;

/// <summary>
/// Standard error response shape returned by all services.
/// Mirrors the RFC 7807 Problem Details convention.
/// </summary>
/// <param name="Type">A URI reference identifying the error type.</param>
/// <param name="Title">A short, human-readable summary.</param>
/// <param name="Status">The HTTP status code.</param>
/// <param name="Detail">A human-readable explanation.</param>
/// <param name="TraceId">The correlation / trace ID for this request.</param>
/// <param name="Errors">Optional — per-field validation errors.</param>
public sealed record ApiErrorResponse(
    string Type,
    string Title,
    int Status,
    string Detail,
    string? TraceId,
    IReadOnlyDictionary<string, string[]>? Errors);

/// <summary>
/// Helper to create standard error responses.
/// </summary>
public static class ApiErrorResponseFactory
{
    public static ApiErrorResponse BadRequest(string detail, string? traceId = null,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => new("https://httpstatuses.io/400", "Bad Request", 400, detail, traceId, errors);

    public static ApiErrorResponse Unauthorized(string detail, string? traceId = null)
        => new("https://httpstatuses.io/401", "Unauthorized", 401, detail, traceId, null);

    public static ApiErrorResponse Forbidden(string detail, string? traceId = null)
        => new("https://httpstatuses.io/403", "Forbidden", 403, detail, traceId, null);

    public static ApiErrorResponse NotFound(string detail, string? traceId = null)
        => new("https://httpstatuses.io/404", "Not Found", 404, detail, traceId, null);

    public static ApiErrorResponse InternalError(string detail, string? traceId = null)
        => new("https://httpstatuses.io/500", "Internal Server Error", 500, detail, traceId, null);
}