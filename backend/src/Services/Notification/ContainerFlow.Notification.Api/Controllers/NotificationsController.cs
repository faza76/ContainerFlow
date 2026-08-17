using ContainerFlow.Notification.Api.Contracts;
using ContainerFlow.Notification.Api.Services;
using ContainerFlow.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContainerFlow.Notification.Api.Controllers;

/// <summary>
/// Read-only notification API — notifications are created by event consumers,
/// never by direct client POSTs.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(
    INotificationService notificationService, ICurrentUser currentUser)
    : ControllerBase
{
    private Guid? CustomerId => currentUser.CustomerId;
    private string? Role => currentUser.Role;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetAll(CancellationToken ct)
    {
        // admin/staff → all; customer → own only
        var customerId = AuthorizationPolicies.CanManageContainers(Role!) ? null : CustomerId;
        var notifications = await notificationService.GetAllAsync(customerId, ct);
        return Ok(notifications.Select(NotificationResponse.From).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationResponse>> GetById(Guid id, CancellationToken ct)
    {
        var customerId = AuthorizationPolicies.CanManageContainers(Role!) ? null : CustomerId;
        var notification = await notificationService.GetAsync(id, customerId, ct);
        if (notification is null) return NotFound();
        return Ok(NotificationResponse.From(notification));
    }
}