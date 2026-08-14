using Serilog;
using Serilog.Events;

namespace ContainerFlow.Shared.Logging;

/// <summary>
/// Shared Serilog setup helper. Every service should call one of these
/// methods at startup so all services log in the same structured format.
///
/// <code>
/// // Program.cs
/// Log.Logger = ContainerFlowLoggingExtensions.CreateLogger(configuration);
///
/// try
/// {
///     var builder = WebApplication.CreateBuilder(args);
///     builder.Host.UseSerilog();
///     // ...
/// }
/// finally
/// {
///     Log.CloseAndFlush();
/// }
/// </code>
/// </summary>
public static class ContainerFlowLoggingExtensions
{
    /// <summary>
    /// Creates a Serilog logger configured with console + rolling file sinks,
    /// enriched with the host environment name and structured output.
    /// </summary>
    /// <param name="environmentName">e.g. "Development", "Production".</param>
    /// <param name="minimumLevel">Default: Information.</param>
    public static ILogger CreateLogger(
        string environmentName = "Production",
        LogEventLevel minimumLevel = LogEventLevel.Information)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", environmentName)
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId,-36} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/containerflow-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {CorrelationId,-36} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}