using ContainerFlow.Booking.Api.Domain;
using ContainerFlow.Contracts.Enums;
using BookingEntity = ContainerFlow.Booking.Api.Domain.Booking;

namespace ContainerFlow.Booking.Tests;

/// <summary>
/// State-machine tests for the booking lifecycle:
/// PENDING → CONFIRMED → IN_PROGRESS → COMPLETED, plus CANCELLED.
/// </summary>
public class BookingStatusTransitionsTests
{
    private static BookingEntity NewBooking(BookingStatus initial = BookingStatus.PENDING) => new()
    {
        Id = Guid.NewGuid(),
        BookingNumber = "BK-TEST-0001",
        CustomerId = Guid.NewGuid(),
        CustomerName = "ACME Logistics",
        Origin = "Rotterdam",
        Destination = "Singapore",
        Cargo = "Auto parts",
        ContainerType = ContainerType.FT40,
        ContainerCount = 2,
        Status = initial,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };

    [Theory]
    [InlineData(BookingStatus.PENDING, BookingStatus.CONFIRMED)]
    [InlineData(BookingStatus.PENDING, BookingStatus.CANCELLED)]
    [InlineData(BookingStatus.CONFIRMED, BookingStatus.IN_PROGRESS)]
    [InlineData(BookingStatus.CONFIRMED, BookingStatus.CANCELLED)]
    [InlineData(BookingStatus.IN_PROGRESS, BookingStatus.COMPLETED)]
    [InlineData(BookingStatus.IN_PROGRESS, BookingStatus.CANCELLED)]
    public void Valid_transitions_succeed(BookingStatus from, BookingStatus to)
    {
        var booking = NewBooking(from);

        booking.TransitionTo(to);

        Assert.Equal(to, booking.Status);
    }

    [Theory]
    [InlineData(BookingStatus.PENDING, BookingStatus.IN_PROGRESS)]
    [InlineData(BookingStatus.PENDING, BookingStatus.COMPLETED)]
    [InlineData(BookingStatus.CONFIRMED, BookingStatus.PENDING)]
    [InlineData(BookingStatus.CONFIRMED, BookingStatus.COMPLETED)]
    [InlineData(BookingStatus.IN_PROGRESS, BookingStatus.CONFIRMED)]
    [InlineData(BookingStatus.COMPLETED, BookingStatus.PENDING)]
    [InlineData(BookingStatus.COMPLETED, BookingStatus.IN_PROGRESS)]
    [InlineData(BookingStatus.CANCELLED, BookingStatus.PENDING)]
    public void Invalid_transitions_throw(BookingStatus from, BookingStatus to)
    {
        var booking = NewBooking(from);

        Assert.Throws<InvalidBookingTransitionException>(() => booking.TransitionTo(to));
    }

    [Fact]
    public void Cancelled_booking_cannot_be_confirmed()
    {
        // The README's own example: confirming a cancelled booking must fail loudly.
        var booking = NewBooking(BookingStatus.CANCELLED);

        var ex = Assert.Throws<InvalidBookingTransitionException>(
            () => booking.TransitionTo(BookingStatus.CONFIRMED));

        Assert.Contains("CANCELLED", ex.Message);
    }

    [Fact]
    public void Same_status_transition_throws()
    {
        var booking = NewBooking(BookingStatus.CONFIRMED);

        Assert.Throws<InvalidBookingTransitionException>(
            () => booking.TransitionTo(BookingStatus.CONFIRMED));
    }
}