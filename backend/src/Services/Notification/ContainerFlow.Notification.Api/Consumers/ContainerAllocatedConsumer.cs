using ContainerFlow.Contracts.Events;
using ContainerFlow.Notification.Api.Messaging;
using ContainerFlow.Notification.Api.Services;
using MassTransit;

namespace ContainerFlow.Notification.Api.Consumers;

public sealed class ContainerAllocatedConsumer(
    INotificationService notifications, ILogger<ContainerAllocatedConsumer> logger)
    : IConsumer<ContainerAllocated>
{
    public async Task Consume(ConsumeContext<ContainerAllocated> context)
    {
        var e = context.Message;
        var (title, message) = NotificationMessageFactory.For(e);
        await notifications.CreateIfNewAsync(
            idempotencyKey: $"{nameof(ContainerAllocated)}:{e.ContainerId}:{e.TimestampUtc:O}",
            eventType: nameof(ContainerAllocated),
            customerId: null, // ContainerAllocated lacks CustomerId; the booking service filters by booking
            bookingId: e.BookingId,
            containerId: e.ContainerId,
            title, message, context.CancellationToken);
    }
}