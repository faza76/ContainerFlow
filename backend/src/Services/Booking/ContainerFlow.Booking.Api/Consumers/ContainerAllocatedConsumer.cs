using ContainerFlow.Booking.Api.Services;
using ContainerFlow.Contracts.Events;
using MassTransit;

namespace ContainerFlow.Booking.Api.Consumers;

/// <summary>
/// Consumes <see cref="ContainerAllocated"/> events published by the Container Service.
/// Advances the booking from CONFIRMED → IN_PROGRESS and records the container reference.
/// </summary>
public sealed class ContainerAllocatedConsumer(
    IBookingService bookingService, ILogger<ContainerAllocatedConsumer> logger)
    : IConsumer<ContainerAllocated>
{
    public async Task Consume(ConsumeContext<ContainerAllocated> context)
    {
        var msg = context.Message;
        await bookingService.HandleContainerAllocatedAsync(
            msg.BookingId, msg.ContainerId, msg.ContainerNumber, context.CancellationToken);
    }
}