using ContainerFlow.Container.Api.Contracts;
using ContainerFlow.Container.Api.Domain;
using ContainerFlow.Container.Api.Persistence;
using ContainerFlow.Contracts.Enums;
using ContainerFlow.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContainerFlow.Container.Api.Services;

public sealed class ContainerService(
    ContainerDbContext db,
    IPublishEndpoint publishEndpoint,
    ILogger<ContainerService> logger)
    : IContainerService
{
    private static readonly ContainerStatus[] ShipmentStageStatuses =
    [
        ContainerStatus.LOADED,
        ContainerStatus.IN_TRANSIT,
        ContainerStatus.DISCHARGED,
        ContainerStatus.DELIVERED
    ];

    public async Task<IReadOnlyList<Domain.Container>> GetAllAsync(
        ContainerStatus? status, CancellationToken ct = default)
    {
        var query = db.Containers.AsQueryable();
        if (status is not null)
            query = query.Where(c => c.Status == status);

        return await query.OrderBy(c => c.ContainerNumber).ToListAsync(ct);
    }

    public async Task<Domain.Container?> GetAsync(Guid id, CancellationToken ct = default)
        => await db.Containers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Domain.Container> RegisterAsync(
        RegisterContainerRequest request, CancellationToken ct = default)
    {
        if (await db.Containers.AnyAsync(c => c.ContainerNumber == request.ContainerNumber, ct))
            throw new InvalidContainerTransitionException(
                $"A container with number {request.ContainerNumber} is already registered.");

        var now = DateTime.UtcNow;
        var container = new Domain.Container
        {
            Id = Guid.NewGuid(),
            ContainerNumber = request.ContainerNumber,
            Type = request.Type,
            Status = ContainerStatus.AVAILABLE,
            Teu = request.Teu ?? DefaultTeu(request.Type),
            Location = null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.Containers.Add(container);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Container {ContainerNumber} registered ({Type})", container.ContainerNumber, container.Type);
        return container;
    }

    public async Task<Domain.Container> AllocateAsync(
        Guid id, AllocateContainerRequest request, Guid correlationId, CancellationToken ct = default)
    {
        var container = await db.Containers.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Container {id} not found.");

        // The README's own business rule: an already-allocated container cannot be
        // allocated to another shipment — the operation must fail loudly.
        if (container.Status != ContainerStatus.AVAILABLE)
            throw new InvalidContainerTransitionException(
                $"Container {container.ContainerNumber} is {container.Status} — it is not available for allocation.");

        container.BookingId = request.BookingId;
        container.BookingNumber = request.BookingNumber;
        container.TransitionTo(ContainerStatus.ALLOCATED);
        await db.SaveChangesAsync(ct);

        await publishEndpoint.Publish(new ContainerAllocated(
            BookingId: request.BookingId,
            BookingNumber: request.BookingNumber,
            ContainerId: container.Id,
            ContainerNumber: container.ContainerNumber,
            ContainerType: container.Type,
            TimestampUtc: DateTime.UtcNow,
            CorrelationId: correlationId), ctx => ctx.CorrelationId = correlationId, ct);

        logger.LogInformation("Container {ContainerNumber} allocated to booking {BookingNumber}",
            container.ContainerNumber, request.BookingNumber);
        return container;
    }

    public async Task<Domain.Container> MoveAsync(
        Guid id, MoveContainerRequest request, Guid correlationId, CancellationToken ct = default)
    {
        var container = await db.Containers.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Container {id} not found.");

        var oldStatus = container.Status;
        container.TransitionTo(request.NewStatus);
        if (request.Location is not null)
            container.Location = request.Location;
        await db.SaveChangesAsync(ct);

        var now = DateTime.UtcNow;
        await publishEndpoint.Publish(new ContainerStatusChanged(
            ContainerId: container.Id,
            ContainerNumber: container.ContainerNumber,
            OldStatus: oldStatus,
            NewStatus: container.Status,
            TimestampUtc: now,
            CorrelationId: correlationId), ctx => ctx.CorrelationId = correlationId, ct);

        // A movement that changes the shipment stage also emits ShipmentStatusChanged.
        if (ShipmentStageStatuses.Contains(container.Status) && container.BookingId is not null)
        {
            await publishEndpoint.Publish(new ShipmentStatusChanged(
                BookingId: container.BookingId.Value,
                BookingNumber: container.BookingNumber ?? string.Empty,
                ContainerId: container.Id,
                ContainerNumber: container.ContainerNumber,
                OldStatus: ShipmentStageFor(oldStatus),
                NewStatus: ShipmentStageFor(container.Status),
                TimestampUtc: now,
                CorrelationId: correlationId), ctx => ctx.CorrelationId = correlationId, ct);
        }

        logger.LogInformation("Container {ContainerNumber} moved {OldStatus} -> {NewStatus}",
            container.ContainerNumber, oldStatus, container.Status);
        return container;
    }

    public async Task<UtilizationResponse> GetUtilizationAsync(CancellationToken ct = default)
    {
        var containers = await db.Containers.Select(c => new { c.Teu, c.Status }).ToListAsync(ct);

        var total = containers.Sum(c => c.Teu);
        if (total == 0)
            return UtilizationResponse.Empty;

        var allocated = containers.Where(c => c.Status != ContainerStatus.AVAILABLE).Sum(c => c.Teu);
        var available = total - allocated;

        return new UtilizationResponse(
            TotalCapacityTeu: total,
            AllocatedTeu: allocated,
            AvailableTeu: available,
            UtilizationPercent: Math.Round(allocated / (double)total * 100, 1));
    }

    private static int DefaultTeu(ContainerType type) => type switch
    {
        ContainerType.FT20 => 1,
        _ => 2
    };

    private static BookingStatus ShipmentStageFor(ContainerStatus status) => status switch
    {
        ContainerStatus.AVAILABLE or ContainerStatus.ALLOCATED or ContainerStatus.IN_YARD
            => BookingStatus.CONFIRMED,
        ContainerStatus.LOADED or ContainerStatus.IN_TRANSIT or ContainerStatus.DISCHARGED
            => BookingStatus.IN_PROGRESS,
        ContainerStatus.DELIVERED => BookingStatus.COMPLETED,
        _ => BookingStatus.PENDING
    };
}