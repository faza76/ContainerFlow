using ContainerFlow.Contracts.Events;
using ContainerFlow.Notification.Api.Messaging;
using ContainerFlow.Notification.Api.Services;
using MassTransit;

namespace ContainerFlow.Notification.Api.Consumers;

public sealed class ShipmentStatusChangedConsumer(
    INotificationService notifications, ILogger<ShipmentStatusChangedConsumer> logger)
    : IConsumer<ShipmentStatusChanged>
{
    public async Task Consume(ConsumeContext<ShipmentStatusChanged> context)
    {
        var e = context.Message;
        var (title, message) = NotificationMessageFactory.For(e);
        await notifications.CreateIfNewAsync(
            idempotencyKey: $"{nameof(ShipmentStatusChanged)}:{e.BookingId}:{e.NewStatus}:{e.TimestampUtc:O}",
            eventType: nameof(ShipmentStatusChanged),
            customerId: null, // no CustomerId on this event
            bookingId: e.BookingId,
            containerId: e.ContainerId,
            title, message, context.CancellationToken);
    }
}