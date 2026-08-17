using ContainerFlow.Container.Api.Domain;
using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Container.Tests;

using ContainerEntity = ContainerFlow.Container.Api.Domain.Container;

public class ContainerStatusTransitionsTests
{
    private static ContainerEntity NewContainer(ContainerStatus initial = ContainerStatus.AVAILABLE) => new()
    {
        Id = Guid.NewGuid(),
        ContainerNumber = "MSCU1234567",
        Type = ContainerType.FT40,
        Status = initial,
        Teu = 2,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };

    [Theory]
    [InlineData(ContainerStatus.AVAILABLE, ContainerStatus.ALLOCATED)]
    [InlineData(ContainerStatus.AVAILABLE, ContainerStatus.IN_YARD)]
    [InlineData(ContainerStatus.ALLOCATED, ContainerStatus.IN_YARD)]
    [InlineData(ContainerStatus.ALLOCATED, ContainerStatus.AVAILABLE)]
    [InlineData(ContainerStatus.IN_YARD, ContainerStatus.LOADED)]
    [InlineData(ContainerStatus.LOADED, ContainerStatus.IN_TRANSIT)]
    [InlineData(ContainerStatus.IN_TRANSIT, ContainerStatus.DISCHARGED)]
    [InlineData(ContainerStatus.DISCHARGED, ContainerStatus.DELIVERED)]
    [InlineData(ContainerStatus.DELIVERED, ContainerStatus.AVAILABLE)]
    public void Valid_transitions_succeed(ContainerStatus from, ContainerStatus to)
    {
        var c = NewContainer(from);
        c.TransitionTo(to);
        Assert.Equal(to, c.Status);
    }

    [Theory]
    [InlineData(ContainerStatus.AVAILABLE, ContainerStatus.DELIVERED)]
    [InlineData(ContainerStatus.AVAILABLE, ContainerStatus.LOADED)]
    [InlineData(ContainerStatus.ALLOCATED, ContainerStatus.LOADED)]
    [InlineData(ContainerStatus.IN_YARD, ContainerStatus.IN_TRANSIT)]
    [InlineData(ContainerStatus.LOADED, ContainerStatus.DELIVERED)]
    [InlineData(ContainerStatus.DELIVERED, ContainerStatus.ALLOCATED)]
    public void Invalid_transitions_throw(ContainerStatus from, ContainerStatus to)
    {
        var c = NewContainer(from);
        Assert.Throws<InvalidContainerTransitionException>(() => c.TransitionTo(to));
    }

    [Fact]
    public void Same_status_transition_throws()
    {
        var c = NewContainer(ContainerStatus.ALLOCATED);
        Assert.Throws<InvalidContainerTransitionException>(() => c.TransitionTo(ContainerStatus.ALLOCATED));
    }
}