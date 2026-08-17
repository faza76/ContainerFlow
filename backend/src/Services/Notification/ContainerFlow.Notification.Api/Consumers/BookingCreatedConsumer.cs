using ContainerFlow.Contracts.Events;
using ContainerFlow.Notification.Api.Messaging;
using ContainerFlow.Notification.Api.Services;
using MassTransit;

namespace ContainerFlow.Notification.Api.Consumers;

public sealed class BookingCreatedConsumer(
    INotificationService notifications, ILogger<BookingCreatedConsumer> logger)
    : IConsumer<BookingCreated>
{
    public async Task Consume(ConsumeContext<BookingCreated> context)
    {
        var e = context.Message;
        var (title, message) = NotificationMessageFactory.For(e);
        await notifications.CreateIfNewAsync(
            idempotencyKey: $"{nameof(BookingCreated)}:{e.BookingId}:{e.TimestampUtc:O}",
            eventType: nameof(BookingCreated),
            customerId: e.CustomerId,
            bookingId: e.BookingId,
            containerId: null,
            title, message, context.CancellationToken);
    }
}