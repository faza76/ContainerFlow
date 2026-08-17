using ContainerFlow.Contracts.Enums;
using ContainerFlow.Contracts.Events;
using ContainerFlow.Notification.Api.Messaging;

namespace ContainerFlow.Notification.Tests;

public class NotificationMessageFactoryTests
{
    [Fact]
    public void BookingConfirmed_returns_correct_message()
    {
        var (title, message) = NotificationMessageFactory.For(new BookingConfirmed(
            Guid.NewGuid(), "BK-2026-0001", Guid.NewGuid(), BookingStatus.CONFIRMED,
            DateTime.UtcNow, Guid.NewGuid()));

        Assert.Equal("Booking confirmed", title);
        Assert.Contains("BK-2026-0001", message);
        Assert.Contains("confirmed", message);
    }

    [Fact]
    public void ContainerStatusChanged_uses_status_text()
    {
        var (title, message) = NotificationMessageFactory.For(new ContainerStatusChanged(
            Guid.NewGuid(), "MSCU1234567",
            ContainerStatus.IN_YARD, ContainerStatus.LOADED,
            DateTime.UtcNow, Guid.NewGuid()));

        Assert.Equal("Container loaded", title);
        Assert.Contains("MSCU1234567", message);
        Assert.Contains("loaded", message);
    }

    [Fact]
    public void ShipmentStatusChanged_departed()
    {
        var (title, message) = NotificationMessageFactory.For(new ShipmentStatusChanged(
            Guid.NewGuid(), "BK-2026-0001", Guid.NewGuid(), "MSCU1234567",
            BookingStatus.CONFIRMED, BookingStatus.IN_PROGRESS,
            DateTime.UtcNow, Guid.NewGuid()));

        Assert.Equal("Shipment update", title);
        Assert.Contains("departed", message);
    }

    [Fact]
    public void ShipmentStatusChanged_delivered()
    {
        var (title, message) = NotificationMessageFactory.For(new ShipmentStatusChanged(
            Guid.NewGuid(), "BK-2026-0001", Guid.NewGuid(), "MSCU1234567",
            BookingStatus.IN_PROGRESS, BookingStatus.COMPLETED,
            DateTime.UtcNow, Guid.NewGuid()));

        Assert.Contains("delivered", message);
    }
}