using ContainerFlow.Notification.Api.Persistence;
using ContainerFlow.Notification.Api.Services;
using ContainerFlow.Contracts.Events;
using ContainerFlow.Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContainerFlow.Notification.Tests;

public class NotificationServiceIdempotencyTests
{
    private static NotificationDbContext CreateDb() => new(
        new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase($"notif-tests-{Guid.NewGuid()}")
            .Options);

    private static (string Key, string EventType, Guid? CustomerId, Guid? BookingId, string Title, string Message) SampleData()
    {
        var key = $"BookingConfirmed:{Guid.NewGuid()}:{DateTime.UtcNow:O}";
        return (key, "BookingConfirmed", Guid.NewGuid(), Guid.NewGuid(), "Test title", "Test message");
    }

    [Fact]
    public async Task CreateIfNew_creates_and_returns()
    {
        var db = CreateDb();
        var service = new NotificationService(db, NullLogger<NotificationService>.Instance);
        var (key, eventType, customerId, bookingId, title, message) = SampleData();

        var result = await service.CreateIfNewAsync(key, eventType, customerId, bookingId, null, title, message);

        Assert.NotNull(result);
        Assert.Equal(key, result.IdempotencyKey);
        Assert.Equal(customerId, result.CustomerId);
    }

    [Fact]
    public async Task CreateIfNew_returns_existing_on_duplicate()
    {
        var db = CreateDb();
        var service = new NotificationService(db, NullLogger<NotificationService>.Instance);
        var (key, eventType, customerId, bookingId, title, message) = SampleData();

        var first = await service.CreateIfNewAsync(key, eventType, customerId, bookingId, null, title, message);
        var second = await service.CreateIfNewAsync(key, eventType, customerId, bookingId, null, title, message);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first.Id, second.Id);
        Assert.Single(db.Notifications);
    }

    [Fact]
    public async Task Different_keys_create_separate_notifications()
    {
        var db = CreateDb();
        var service = new NotificationService(db, NullLogger<NotificationService>.Instance);
        var (key1, eventType, customerId, bookingId, title, message) = SampleData();
        var key2 = $"BookingConfirmed:{Guid.NewGuid()}:{DateTime.UtcNow:O}";

        await service.CreateIfNewAsync(key1, eventType, customerId, bookingId, null, title, message);
        await service.CreateIfNewAsync(key2, eventType, customerId, bookingId, null, title, message);

        Assert.Equal(2, db.Notifications.Count());
    }

    [Fact]
    public async Task Customer_scoping_returns_only_own_notifications()
    {
        var db = CreateDb();
        var service = new NotificationService(db, NullLogger<NotificationService>.Instance);
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();

        await service.CreateIfNewAsync("key1", "E", customerA, null, null, "T", "M");
        await service.CreateIfNewAsync("key2", "E", customerB, null, null, "T", "M");

        var aNotifs = await service.GetAllAsync(customerA);
        var bNotifs = await service.GetAllAsync(customerB);

        Assert.Single(aNotifs);
        Assert.Single(bNotifs);
    }
}