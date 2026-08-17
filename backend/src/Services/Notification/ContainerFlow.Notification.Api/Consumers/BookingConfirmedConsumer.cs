using ContainerFlow.Contracts.Events;
using ContainerFlow.Notification.Api.Messaging;
using ContainerFlow.Notification.Api.Services;
using MassTransit;

namespace ContainerFlow.Notification.Api.Consumers;

public sealed class BookingConfirmedConsumer(
    INotificationService notifications, ILogger<BookingConfirmedConsumer> logger)
    : IConsumer<BookingConfirmed>
{
    public async Task Consume(ConsumeContext<BookingConfirmed> context)
    {
        var e = context.Message;
        var (title, message) = NotificationMessageFactory.For(e);
        await notifications.CreateIfNewAsync(
            idempotencyKey: $"{nameof(BookingConfirmed)}:{e.BookingId}:{e.TimestampUtc:O}",
            eventType: nameof(BookingConfirmed),
            customerId: e.CustomerId,
            bookingId: e.BookingId,
            containerId: null,
            title, message, context.CancellationToken);
    }
}