# ContainerFlow.Shared

Cross-cutting building blocks referenced by every ContainerFlow service **and** the
Gateway. Contains no single-service business logic.

## What's inside

| Module | Namespace | Purpose |
|---|---|---|
| Auth | `ContainerFlow.Shared.Auth` | JWT claim-type constants (`AuthConstants`) and role-checking helpers (`AuthorizationPolicies`) |
| Health | `ContainerFlow.Shared.Health` | `AddContainerFlowHealthChecks()` base health-check extension |
| Correlation | `ContainerFlow.Shared.Correlation` | `CorrelationIdMiddleware` + `UseCorrelationId()` / `GetCorrelationId()` |
| Logging | `ContainerFlow.Shared.Logging` | `CreateLogger()` Serilog setup (console + rolling file, CorrelationId scope) |
| API conventions | `ContainerFlow.Shared.Api` | `ApiErrorResponse` problem-details shape + factory |

## Auth

```csharp
// Claim constants
var userId = claims.FindFirst(AuthConstants.ClaimSub)?.Value;
var role   = claims.FindFirst(AuthConstants.ClaimRole)?.Value;

// Role checks (shared vocabulary — enforce policy in your own code)
if (AuthorizationPolicies.CanManageBookings(role)) { /* ... */ }
if (AuthorizationPolicies.CanViewNotifications(role)) { /* ... */ }
```

Roles: `admin` (manage everything + notification users), `staff` (manage bookings &
containers, view notifications), `customer` (create/view own bookings, view own
notifications).

## Health checks

```csharp
// Program.cs — add the shared base, then your service-specific checks:
builder.Services.AddContainerFlowHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Notification Service adds RabbitMQ on top:
builder.Services.AddContainerFlowHealthChecks()
    .AddNpgSql(...)
    .AddRabbitMQ(rabbitUri);
```

Don't forget to map the endpoint:

```csharp
app.MapHealthChecks("/health");
```

## Correlation ID

```csharp
// Program.cs
app.UseCorrelationId();           // reads X-Correlation-Id header or generates a Guid,
                                  // adds it to the response header + logging scope

// Anywhere you have HttpContext:
var correlationId = HttpContext.GetCorrelationId();

// Attach to a published event:
await bus.Publish(new BookingCreated(/* ... */, CorrelationId: correlationId ?? Guid.NewGuid()));
```

## Logging

```csharp
// Program.cs
Log.Logger = ContainerFlowLoggingExtensions.CreateLogger(
    environmentName: builder.Environment.EnvironmentName);
builder.Host.UseSerilog();
// ...
Log.CloseAndFlush();
```

Every log line carries a `CorrelationId` property (populated by the middleware) and
the environment name.

## API error responses

```csharp
// Services return this shape on errors (mirrors RFC 7807):
var err = ApiErrorResponseFactory.NotFound($"Booking {id} not found", traceId);
```

## Framework

- Targets `net10.0` (matches the `mcr.microsoft.com/dotnet/sdk:8.0` build images used in
  `infrastructure/docker/`).
- References only the ASP.NET Core shared framework + Serilog sinks — **no**
  PostgreSQL/EF Core, no RabbitMQ transport, no MassTransit.
