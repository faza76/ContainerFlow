using ContainerFlow.Booking.Api.Contracts;
using ContainerFlow.Booking.Api.Services;
using ContainerFlow.Booking.Api.Domain;
using ContainerFlow.Shared.Auth;
using ContainerFlow.Shared.Correlation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContainerFlow.Booking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController(
    IBookingService bookingService, ICurrentUser currentUser, ILogger<BookingsController> logger)
    : ControllerBase
{
    private Guid? CustomerId => currentUser.CustomerId;
    private string? Role => currentUser.Role;
    private Guid CorrelationId => HttpContext.GetCorrelationId() is { } cid && Guid.TryParse(cid, out var g) ? g : Guid.NewGuid();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanCreateBookings(Role!))
            return Forbid();

        if (currentUser.Id is null)
            return Unauthorized();

        var booking = await bookingService.CreateAsync(request, currentUser.Id.Value, CorrelationId, ct);
        logger.LogInformation("Booking {BookingNumber} created.", booking.BookingNumber);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, BookingResponse.From(booking));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll(CancellationToken ct)
    {
        var customerId = AuthorizationPolicies.CanManageBookings(Role!) ? null : CustomerId;
        var bookings = await bookingService.GetAllAsync(customerId, ct);
        return Ok(bookings.Select(BookingResponse.From).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id, CancellationToken ct)
    {
        var customerId = AuthorizationPolicies.CanManageBookings(Role!) ? null : CustomerId;
        var booking = await bookingService.GetAsync(id, customerId, ct);
        if (booking is null) return NotFound();
        return Ok(BookingResponse.From(booking));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> Update(Guid id, [FromBody] UpdateBookingRequest request, CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanManageBookings(Role!))
            return Forbid();

        var booking = await bookingService.UpdateAsync(id, request, ct);
        if (booking is null) return NotFound();
        return Ok(BookingResponse.From(booking));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<BookingResponse>> Confirm(Guid id, CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanManageBookings(Role!))
            return Forbid();

        try
        {
            var booking = await bookingService.ConfirmAsync(id, CorrelationId, ct);
            if (booking is null) return NotFound();
            return Ok(BookingResponse.From(booking));
        }
        catch (InvalidBookingTransitionException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<BookingResponse>> Cancel(Guid id, CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanManageBookings(Role!))
            return Forbid();

        try
        {
            var booking = await bookingService.CancelAsync(id, ct);
            if (booking is null) return NotFound();
            return Ok(BookingResponse.From(booking));
        }
        catch (InvalidBookingTransitionException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}