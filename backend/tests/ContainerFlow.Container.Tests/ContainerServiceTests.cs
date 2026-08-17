using ContainerFlow.Container.Api.Contracts;
using ContainerFlow.Container.Api.Domain;
using ContainerFlow.Container.Api.Persistence;
using ContainerEntity = ContainerFlow.Container.Api.Domain.Container;
using ContainerFlow.Container.Api.Services;
using ContainerFlow.Contracts.Enums;
using ContainerFlow.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ContainerFlow.Container.Tests;

public class ContainerServiceTests
{
    private static ContainerDbContext CreateDb() => new(
        new DbContextOptionsBuilder<ContainerDbContext>()
            .UseInMemoryDatabase($"container-tests-{Guid.NewGuid()}")
            .Options);

    private static RegisterContainerRequest RegisterReq(string number = "MSCU1234567") => new(
        ContainerNumber: number,
        Type: ContainerType.FT40,
        Teu: null);

    [Fact]
    public async Task Register_creates_container_as_available()
    {
        var db = CreateDb();
        var service = new ContainerService(db, Mock.Of<IPublishEndpoint>(), NullLogger<ContainerService>.Instance);

        var c = await service.RegisterAsync(RegisterReq());

        Assert.Equal(ContainerStatus.AVAILABLE, c.Status);
        Assert.Equal(2, c.Teu);
        Assert.StartsWith("MSCU", c.ContainerNumber);
    }

    [Fact]
    public async Task Register_duplicate_number_throws()
    {
        var db = CreateDb();
        var service = new ContainerService(db, Mock.Of<IPublishEndpoint>(), NullLogger<ContainerService>.Instance);

        await service.RegisterAsync(RegisterReq());
        await Assert.ThrowsAsync<InvalidContainerTransitionException>(
            () => service.RegisterAsync(RegisterReq()));
    }

    [Fact]
    public async Task Allocate_fails_for_non_available_container()
    {
        // "Given a container is already allocated, when another shipment attempts
        //  to allocate it, then the operation should fail."
        var db = CreateDb();
        var service = new ContainerService(db, Mock.Of<IPublishEndpoint>(), NullLogger<ContainerService>.Instance);

        var c = await service.RegisterAsync(RegisterReq());
        await service.AllocateAsync(c.Id, new AllocateContainerRequest(Guid.NewGuid(), "BK-1"), Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<InvalidContainerTransitionException>(
            () => service.AllocateAsync(c.Id, new AllocateContainerRequest(Guid.NewGuid(), "BK-2"), Guid.NewGuid()));
        Assert.Contains("not available", ex.Message);
    }

    [Fact]
    public async Task Allocate_publishes_ContainerAllocated()
    {
        var db = CreateDb();
        var publish = new Mock<IPublishEndpoint>();
        var service = new ContainerService(db, publish.Object, NullLogger<ContainerService>.Instance);

        var c = await service.RegisterAsync(RegisterReq());
        var bookingId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        await service.AllocateAsync(c.Id, new AllocateContainerRequest(bookingId, "BK-1"), correlationId);

        var published = publish.Invocations
            .Select(i => i.Arguments.FirstOrDefault())
            .OfType<ContainerAllocated>()
            .Single();
        Assert.Equal(bookingId, published.BookingId);
        Assert.Equal(c.Id, published.ContainerId);
        Assert.Equal(correlationId, published.CorrelationId);
    }

    [Fact]
    public async Task Move_publishes_ContainerStatusChanged_and_ShipmentStatusChanged()
    {
        var db = CreateDb();
        var publish = new Mock<IPublishEndpoint>();
        var service = new ContainerService(db, publish.Object, NullLogger<ContainerService>.Instance);

        var c = await service.RegisterAsync(RegisterReq());
        await service.AllocateAsync(c.Id, new AllocateContainerRequest(Guid.NewGuid(), "BK-1"), Guid.NewGuid());

        // Move: IN_YARD -> LOADED (shipment stage change)
        await service.MoveAsync(c.Id, new MoveContainerRequest(ContainerStatus.IN_YARD, null), Guid.NewGuid());
        var moved = await service.MoveAsync(c.Id, new MoveContainerRequest(ContainerStatus.LOADED, "Port of Rotterdam"), Guid.NewGuid());

        // ContainerStatusChanged published
        Assert.Single(publish.Invocations
            .Select(i => i.Arguments.FirstOrDefault())
            .OfType<ContainerStatusChanged>()
            .Where(e => e.NewStatus == ContainerStatus.LOADED));

        // ShipmentStatusChanged also published
        var shipmentEvts = publish.Invocations
            .Select(i => i.Arguments.FirstOrDefault())
            .OfType<ShipmentStatusChanged>()
            .ToList();
        Assert.Contains(shipmentEvts, e => e.NewStatus == BookingStatus.IN_PROGRESS);
        Assert.Equal("Port of Rotterdam", moved.Location);
    }

    [Fact]
    public async Task Utilization_returns_correct_percentages()
    {
        var db = CreateDb();
        var service = new ContainerService(db, Mock.Of<IPublishEndpoint>(), NullLogger<ContainerService>.Instance);

        // 1 container, 2 TEU, available
        await service.RegisterAsync(RegisterReq("MSCU0000001"));
        // 1 container, 2 TEU, later allocated
        var c2 = await service.RegisterAsync(RegisterReq("MSCU0000002"));
        await service.AllocateAsync(c2.Id, new AllocateContainerRequest(Guid.NewGuid(), "BK-1"), Guid.NewGuid());

        var u = await service.GetUtilizationAsync();

        Assert.Equal(4, u.TotalCapacityTeu);      // 2 + 2
        Assert.Equal(2, u.AllocatedTeu);           // only c2 allocated
        Assert.Equal(2, u.AvailableTeu);            // c1 available
        Assert.Equal(50.0, u.UtilizationPercent);
    }

    [Fact]
    public async Task Utilization_empty_when_no_containers()
    {
        var db = CreateDb();
        var service = new ContainerService(db, Mock.Of<IPublishEndpoint>(), NullLogger<ContainerService>.Instance);

        var u = await service.GetUtilizationAsync();

        Assert.Equal(0, u.TotalCapacityTeu);
        Assert.Equal(0, u.UtilizationPercent);
    }
}