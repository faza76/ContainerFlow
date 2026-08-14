# ContainerFlow.Contracts

Shared message/event contracts and enums used by every ContainerFlow service via
MassTransit. **No business logic** — plain immutable records and enums only.

## Projects that reference this

- `ContainerFlow.Booking.Api`
- `ContainerFlow.Container.Api`
- `ContainerFlow.Notification.Api`
- `ContainerFlow.Gateway` (consumes events for SSE push, if needed)

## Integration events (namespace `ContainerFlow.Contracts.Events`)

| Event | Key fields |
|---|---|
| `BookingCreated` | `BookingId`, `BookingNumber`, `CustomerId`, `CustomerName`, `ContainerType`, `ContainerCount`, `TimestampUtc`, `CorrelationId` |
| `BookingConfirmed` | `BookingId`, `BookingNumber`, `CustomerId`, `Status`, `TimestampUtc`, `CorrelationId` |
| `ContainerAllocated` | `BookingId`, `BookingNumber`, `ContainerId`, `ContainerNumber`, `ContainerType`, `TimestampUtc`, `CorrelationId` |
| `ShipmentStatusChanged` | `BookingId`, `BookingNumber`, `ContainerId?`, `ContainerNumber?`, `OldStatus`, `NewStatus`, `TimestampUtc`, `CorrelationId` |
| `ContainerStatusChanged` | `ContainerId`, `ContainerNumber`, `OldStatus`, `NewStatus`, `TimestampUtc`, `CorrelationId` |

Every event carries:

- `TimestampUtc` (`DateTime`, UTC)
- `CorrelationId` (`Guid`) — used for end-to-end tracing across
  gateway → service → RabbitMQ → consumer.

## Enums (namespace `ContainerFlow.Contracts.Enums`)

```text
BookingStatus:    PENDING, CONFIRMED, CANCELLED, IN_PROGRESS, COMPLETED
ContainerType:    FT20, FT40, FT40HC, REEFER
ContainerStatus:  AVAILABLE, ALLOCATED, IN_YARD, LOADED, IN_TRANSIT, DISCHARGED, DELIVERED
Role:             admin, staff, customer
```

> `ContainerType` member names use a `FT` prefix (`FT20`, `FT40`, `FT40HC`) to keep the
> enum member valid C# identifiers — the *meaning* matches the plan's `20FT`, `40FT`,
> `40HC` values. Do not rename or reorder these without coordinating with every consumer.

## Usage

```csharp
// Publishing (any service):
await bus.Publish(new BookingCreated(
    BookingId: Guid.NewGuid(),
    BookingNumber: "BK-2026-00042",
    CustomerId: customerId,
    CustomerName: "ACME Logistics",
    ContainerType: ContainerType.FT40,
    ContainerCount: 2,
    TimestampUtc: DateTime.UtcNow,
    CorrelationId: correlationId));
```
