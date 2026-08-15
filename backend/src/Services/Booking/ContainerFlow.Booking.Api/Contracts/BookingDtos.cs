using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Booking.Api.Contracts;

public sealed record CreateBookingRequest(
    string CustomerName,
    string Origin,
    string Destination,
    string Cargo,
    ContainerType ContainerType,
    int ContainerCount,
    string? Vessel,
    string? Voyage);

public sealed record UpdateBookingRequest(
    string Origin,
    string Destination,
    string Cargo,
    ContainerType ContainerType,
    int ContainerCount,
    string? Vessel,
    string? Voyage);

public sealed record ContainerReferenceResponse(Guid? ContainerId, string ContainerNumber);

public sealed record BookingResponse(
    Guid Id,
    string BookingNumber,
    Guid CustomerId,
    string CustomerName,
    string Origin,
    string Destination,
    string Cargo,
    ContainerType ContainerType,
    int ContainerCount,
    string? Vessel,
    string? Voyage,
    BookingStatus Status,
    IReadOnlyList<ContainerReferenceResponse> Containers,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static BookingResponse From(ContainerFlow.Booking.Api.Domain.Booking booking) => new(
        booking.Id,
        booking.BookingNumber,
        booking.CustomerId,
        booking.CustomerName,
        booking.Origin,
        booking.Destination,
        booking.Cargo,
        booking.ContainerType,
        booking.ContainerCount,
        booking.Vessel,
        booking.Voyage,
        booking.Status,
        booking.Containers
            .Select(c => new ContainerReferenceResponse(c.ContainerId, c.ContainerNumber))
            .ToList(),
        booking.CreatedAtUtc,
        booking.UpdatedAtUtc);
}