using ContainerFlow.Contracts.Enums;
using ContainerFlow.Contracts.Events;

namespace ContainerFlow.Notification.Api.Messaging;

/// <summary>
/// Maps integration events to human-readable notification text.
/// Several statuses map to distinct sentences — driven by the event's status field,
/// never hardcoded per message.
/// </summary>
public static class NotificationMessageFactory
{
    public static (string Title, string Message) For(BookingCreated e) => (
        "Booking created",
        $"Booking {e.BookingNumber} has been created for {e.CustomerName}.");

    public static (string Title, string Message) For(BookingConfirmed e) => (
        "Booking confirmed",
        $"Booking {e.BookingNumber} has been confirmed.");

    public static (string Title, string Message) For(ContainerAllocated e) => (
        "Container allocated",
        $"Container {e.ContainerNumber} ({e.ContainerType}) has been allocated to booking {e.BookingNumber}.");

    public static (string Title, string Message) For(ContainerStatusChanged e)
    {
        var verb = e.NewStatus switch
        {
            ContainerStatus.AVAILABLE => "available",
            ContainerStatus.ALLOCATED => "allocated",
            ContainerStatus.IN_YARD => "in yard",
            ContainerStatus.LOADED => "loaded",
            ContainerStatus.IN_TRANSIT => "in transit",
            ContainerStatus.DISCHARGED => "discharged",
            ContainerStatus.DELIVERED => "delivered",
            _ => e.NewStatus.ToString().ToLowerInvariant()
        };
        return (
            $"Container {verb}",
            $"Container {e.ContainerNumber} is now {verb} (was {e.OldStatus.ToString().ToLowerInvariant().Replace('_', ' ')}).");
    }

    public static (string Title, string Message) For(ShipmentStatusChanged e)
    {
        var text = e.NewStatus switch
        {
            BookingStatus.CONFIRMED => "Shipment awaiting departure",
            BookingStatus.IN_PROGRESS => "Shipment departed",
            BookingStatus.COMPLETED => "Container delivered",
            BookingStatus.CANCELLED => "Shipment cancelled",
            _ => $"Shipment status changed to {e.NewStatus}"
        };
        return (
            "Shipment update",
            $"{text} — booking {e.BookingNumber}.");
    }
}