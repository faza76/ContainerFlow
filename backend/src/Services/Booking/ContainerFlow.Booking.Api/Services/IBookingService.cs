using ContainerFlow.Booking.Api.Contracts;
using ContainerFlow.Booking.Api.Domain;

namespace ContainerFlow.Booking.Api.Services;

/// <summary>
/// NOTE: the entity is referenced as <c>Domain.Booking</c> because the simple name
/// <c>Booking</c> is shadowed by the <c>ContainerFlow.Booking</c> namespace segment.
/// </summary>
public interface IBookingService
{
    Task<Domain.Booking> CreateAsync(CreateBookingRequest request, Guid customerId, Guid correlationId, CancellationToken ct = default);
    Task<Domain.Booking?> GetAsync(Guid id, Guid? customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Booking>> GetAllAsync(Guid? customerId, CancellationToken ct = default);
    Task<Domain.Booking?> UpdateAsync(Guid id, UpdateBookingRequest request, CancellationToken ct = default);
    Task<Domain.Booking?> ConfirmAsync(Guid id, Guid correlationId, CancellationToken ct = default);
    Task<Domain.Booking?> CancelAsync(Guid id, CancellationToken ct = default);
    Task<Domain.Booking?> HandleContainerAllocatedAsync(Guid bookingId, Guid? containerId, string containerNumber, CancellationToken ct = default);
}