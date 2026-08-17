using ContainerFlow.Booking.Api.Contracts;
using ContainerFlow.Booking.Api.Domain;
using ContainerFlow.Booking.Api.Persistence;
using ContainerFlow.Contracts.Enums;
using ContainerFlow.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContainerFlow.Booking.Api.Services;

public sealed class BookingService(
    BookingDbContext db,
    IPublishEndpoint publishEndpoint,
    ILogger<BookingService> logger)
    : IBookingService
{
    public async Task<Domain.Booking> CreateAsync(
        CreateBookingRequest request, Guid customerId, Guid correlationId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var booking = new Domain.Booking
        {
            Id = Guid.NewGuid(),
            BookingNumber = await GenerateBookingNumberAsync(now, ct),
            CustomerId = customerId,
            CustomerName = request.CustomerName,
            Origin = request.Origin,
            Destination = request.Destination,
            Cargo = request.Cargo,
            ContainerType = request.ContainerType,
            ContainerCount = request.ContainerCount,
            Vessel = request.Vessel,
            Voyage = request.Voyage,
            Status = BookingStatus.PENDING,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        // Publish AFTER the DB transaction commits — downstream services must only
        // ever hear about bookings that actually exist.
        await publishEndpoint.Publish(new BookingCreated(
            BookingId: booking.Id,
            BookingNumber: booking.BookingNumber,
            CustomerId: booking.CustomerId,
            CustomerName: booking.CustomerName,
            ContainerType: booking.ContainerType,
            ContainerCount: booking.ContainerCount,
            TimestampUtc: DateTime.UtcNow,
            CorrelationId: correlationId), ctx => ctx.CorrelationId = correlationId, ct);

        logger.LogInformation("Booking {BookingNumber} created for customer {CustomerId}",
            booking.BookingNumber, booking.CustomerId);
        return booking;
    }

    public async Task<Domain.Booking?> GetAsync(Guid id, Guid? customerId, CancellationToken ct = default)
    {
        var query = db.Bookings.Include(b => b.Containers).AsQueryable();
        if (customerId is not null)
            query = query.Where(b => b.CustomerId == customerId);

        return await query.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyList<Domain.Booking>> GetAllAsync(Guid? customerId, CancellationToken ct = default)
    {
        var query = db.Bookings.Include(b => b.Containers).AsQueryable();
        if (customerId is not null)
            query = query.Where(b => b.CustomerId == customerId);

        return await query.OrderByDescending(b => b.CreatedAtUtc).ToListAsync(ct);
    }

    public async Task<Domain.Booking?> UpdateAsync(Guid id, UpdateBookingRequest request, CancellationToken ct = default)
    {
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (booking is null) return null;

        booking.Origin = request.Origin;
        booking.Destination = request.Destination;
        booking.Cargo = request.Cargo;
        booking.ContainerType = request.ContainerType;
        booking.ContainerCount = request.ContainerCount;
        booking.Vessel = request.Vessel;
        booking.Voyage = request.Voyage;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return booking;
    }

    public async Task<Domain.Booking?> ConfirmAsync(Guid id, Guid correlationId, CancellationToken ct = default)
    {
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (booking is null) return null;

        // Invalid transitions throw (e.g. confirming a CANCELLED booking) and no event
        // is published — the controller surfaces the error before anything is sent.
        booking.TransitionTo(BookingStatus.CONFIRMED);
        await db.SaveChangesAsync(ct);

        await publishEndpoint.Publish(new BookingConfirmed(
            BookingId: booking.Id,
            BookingNumber: booking.BookingNumber,
            CustomerId: booking.CustomerId,
            Status: booking.Status,
            TimestampUtc: DateTime.UtcNow,
            CorrelationId: correlationId), ctx => ctx.CorrelationId = correlationId, ct);

        logger.LogInformation("Booking {BookingNumber} confirmed", booking.BookingNumber);
        return booking;
    }

    public async Task<Domain.Booking?> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (booking is null) return null;

        booking.TransitionTo(BookingStatus.CANCELLED);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingNumber} cancelled", booking.BookingNumber);
        return booking;
    }

    /// <summary>
    /// Called by the <c>ContainerAllocated</c> consumer: records the allocated
    /// container and advances CONFIRMED → IN_PROGRESS. Idempotent — redelivered
    /// allocations or allocations for non-CONFIRMED bookings are ignored.
    /// </summary>
    public async Task<Domain.Booking?> HandleContainerAllocatedAsync(
        Guid bookingId, Guid? containerId, string containerNumber, CancellationToken ct = default)
    {
        var booking = await db.Bookings.Include(b => b.Containers)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking is null)
        {
            logger.LogWarning("ContainerAllocated for unknown booking {BookingId}", bookingId);
            return null;
        }

        if (booking.Status != BookingStatus.CONFIRMED)
            return booking; // ignore redeliveries / out-of-order allocations

        var allocation = new BookingContainer
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            ContainerId = containerId,
            ContainerNumber = containerNumber
        };
        booking.Containers.Add(allocation);
        db.BookingContainers.Add(allocation); // explicit Add — robust across providers

        booking.TransitionTo(BookingStatus.IN_PROGRESS);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingNumber} advanced to IN_PROGRESS (container {ContainerNumber})",
            booking.BookingNumber, containerNumber);
        return booking;
    }

    private async Task<string> GenerateBookingNumberAsync(DateTime now, CancellationToken ct)
    {
        var year = now.Year;
        var count = await db.Bookings.CountAsync(b => b.CreatedAtUtc.Year == year, ct);
        return $"BK-{year}-{count + 1:0000}";
    }
}