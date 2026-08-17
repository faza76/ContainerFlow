using ContainerFlow.Booking.Api.Contracts;
using ContainerFlow.Booking.Api.Domain;
using ContainerFlow.Booking.Api.Persistence;
using ContainerFlow.Booking.Api.Services;
using ContainerFlow.Contracts.Enums;
using ContainerFlow.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ContainerFlow.Booking.Tests;

/// <summary>
/// Service-level tests: events are published only after a successful DB commit,
/// and invalid operations never publish anything.
/// </summary>
public class BookingServiceTests
{
    private static BookingDbContext CreateDb() => new(
        new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase($"booking-tests-{Guid.NewGuid()}")
            .Options);

    private static CreateBookingRequest Request() => new(
        CustomerName: "ACME Logistics",
        Origin: "Rotterdam",
        Destination: "Singapore",
        Cargo: "Auto parts",
        ContainerType: ContainerType.FT40,
        ContainerCount: 2,
        Vessel: "MV Maersk Voyager",
        Voyage: "V124");

    [Fact]
    public async Task Create_publishes_BookingCreated_with_correlation_id()
    {
        var db = CreateDb();
        var publish = new Mock<IPublishEndpoint>();
        var service = new BookingService(db, publish.Object, NullLogger<BookingService>.Instance);
        var correlationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var booking = await service.CreateAsync(Request(), customerId, correlationId);

        Assert.Equal(BookingStatus.PENDING, booking.Status);
        Assert.StartsWith("BK-", booking.BookingNumber);
        Assert.Equal(customerId, booking.CustomerId);
        var published = publish.Invocations
            .Select(i => i.Arguments.FirstOrDefault())
            .OfType<BookingCreated>()
            .Single();
        Assert.Equal(correlationId, published.CorrelationId);
        Assert.Equal(booking.Id, published.BookingId);
    }

    [Fact]
    public async Task Confirm_publishes_BookingConfirmed_after_commit()
    {
        var db = CreateDb();
        var publish = new Mock<IPublishEndpoint>();
        var service = new BookingService(db, publish.Object, NullLogger<BookingService>.Instance);
        var booking = await service.CreateAsync(Request(), Guid.NewGuid(), Guid.NewGuid());

        await service.ConfirmAsync(booking.Id, Guid.NewGuid());

        Assert.Single(publish.Invocations
            .Select(i => i.Arguments.FirstOrDefault())
            .OfType<BookingConfirmed>());
        var persisted = await db.Bookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.CONFIRMED, persisted!.Status);
    }

    [Fact]
    public async Task Confirm_cancelled_booking_fails_and_publishes_nothing()
    {
        // "Given a booking is CANCELLED, when a confirm operation is attempted,
        //  then the operation fails and no event is published."
        var db = CreateDb();
        var publish = new Mock<IPublishEndpoint>();
        var service = new BookingService(db, publish.Object, NullLogger<BookingService>.Instance);
        var booking = await service.CreateAsync(Request(), Guid.NewGuid(), Guid.NewGuid());
        await service.CancelAsync(booking.Id);

        await Assert.ThrowsAsync<InvalidBookingTransitionException>(
            () => service.ConfirmAsync(booking.Id, Guid.NewGuid()));

        Assert.Empty(publish.Invocations
            .Select(i => i.Arguments.FirstOrDefault())
            .OfType<BookingConfirmed>());
        var persisted = await db.Bookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.CANCELLED, persisted!.Status);
    }

    [Fact]
    public async Task Customer_scoping_returns_only_own_bookings()
    {
        var db = CreateDb();
        var service = new BookingService(db, new Mock<IPublishEndpoint>().Object, NullLogger<BookingService>.Instance);
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();

        var a = await service.CreateAsync(Request(), customerA, Guid.NewGuid());
        await service.CreateAsync(Request(), customerB, Guid.NewGuid());

        var visible = await service.GetAllAsync(customerA);

        var ids = visible.Select(b => b.Id).ToList();
        Assert.Contains(a.Id, ids);
        Assert.Single(visible);
    }

    [Fact]
    public async Task HandleContainerAllocated_advances_confirmed_booking_to_in_progress()
    {
        var db = CreateDb();
        var service = new BookingService(db, new Mock<IPublishEndpoint>().Object, NullLogger<BookingService>.Instance);
        var booking = await service.CreateAsync(Request(), Guid.NewGuid(), Guid.NewGuid());
        await service.ConfirmAsync(booking.Id, Guid.NewGuid());

        var updated = await service.HandleContainerAllocatedAsync(booking.Id, Guid.NewGuid(), "MSCU1234567");

        Assert.Equal(BookingStatus.IN_PROGRESS, updated!.Status);
        Assert.Single(updated.Containers);
    }

    [Fact]
    public async Task HandleContainerAllocated_is_idempotent_on_redelivery()
    {
        var db = CreateDb();
        var service = new BookingService(db, new Mock<IPublishEndpoint>().Object, NullLogger<BookingService>.Instance);
        var booking = await service.CreateAsync(Request(), Guid.NewGuid(), Guid.NewGuid());
        await service.ConfirmAsync(booking.Id, Guid.NewGuid());
        var containerId = Guid.NewGuid();

        await service.HandleContainerAllocatedAsync(booking.Id, containerId, "MSCU1234567");
        // Simulate a redelivered message
        await service.HandleContainerAllocatedAsync(booking.Id, containerId, "MSCU1234567");

        var persisted = await db.Bookings.Include(b => b.Containers).SingleAsync(b => b.Id == booking.Id);
        Assert.Equal(BookingStatus.IN_PROGRESS, persisted.Status);
        Assert.Single(persisted.Containers);
    }
}