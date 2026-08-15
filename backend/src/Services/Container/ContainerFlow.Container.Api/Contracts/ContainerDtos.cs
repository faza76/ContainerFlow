using ContainerFlow.Container.Api.Domain;
using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Container.Api.Contracts;

public sealed record RegisterContainerRequest(
    string ContainerNumber,
    ContainerType Type,
    int? Teu);

public sealed record AllocateContainerRequest(
    Guid BookingId,
    string BookingNumber);

public sealed record MoveContainerRequest(
    ContainerStatus NewStatus,
    string? Location);

public sealed record ContainerResponse(
    Guid Id,
    string ContainerNumber,
    ContainerType Type,
    ContainerStatus Status,
    int Teu,
    Guid? BookingId,
    string? BookingNumber,
    string? Location,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static ContainerResponse From(Domain.Container c) => new(
        c.Id, c.ContainerNumber, c.Type, c.Status, c.Teu,
        c.BookingId, c.BookingNumber, c.Location, c.CreatedAtUtc, c.UpdatedAtUtc);
}

/// <summary>Shape for <c>GET /api/containers/utilization</c> — chartable directly.</summary>
public sealed record UtilizationResponse(
    int TotalCapacityTeu,
    int AllocatedTeu,
    int AvailableTeu,
    double UtilizationPercent)
{
    public static UtilizationResponse Empty => new(0, 0, 0, 0);
}