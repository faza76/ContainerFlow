using ContainerFlow.Notification.Api.Domain;

namespace ContainerFlow.Notification.Api.Contracts;

public sealed record NotificationResponse(
    Guid Id,
    Guid? CustomerId,
    Guid? BookingId,
    Guid? ContainerId,
    string EventType,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAtUtc)
{
    public static NotificationResponse From(Domain.Notification n) => new(
        n.Id, n.CustomerId, n.BookingId, n.ContainerId,
        n.EventType, n.Title, n.Message, n.IsRead, n.CreatedAtUtc);
}