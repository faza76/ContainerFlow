using ContainerFlow.Container.Api.Contracts;
using ContainerFlow.Container.Api.Domain;
using ContainerFlow.Container.Api.Services;
using ContainerFlow.Contracts.Enums;
using ContainerFlow.Shared.Auth;
using ContainerFlow.Shared.Correlation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContainerFlow.Container.Api.Controllers;

[ApiController]
[Route("api/containers")]
[Authorize]
public sealed class ContainersController(
    IContainerService containerService, ICurrentUser currentUser, ILogger<ContainersController> logger)
    : ControllerBase
{
    private string? Role => currentUser.Role;
    private Guid CorrelationId => HttpContext.GetCorrelationId() is { } cid && Guid.TryParse(cid, out var g) ? g : Guid.NewGuid();

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContainerResponse>>> GetAll(
        [FromQuery] ContainerStatus? status, CancellationToken ct)
    {
        // Auth table: customers have NO container access (“—”).
        if (!AuthorizationPolicies.CanManageContainers(Role!))
            return Forbid();

        var containers = await containerService.GetAllAsync(status, ct);
        return Ok(containers.Select(ContainerResponse.From).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContainerResponse>> GetById(Guid id, CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanManageContainers(Role!))
            return Forbid();

        var container = await containerService.GetAsync(id, ct);
        if (container is null) return NotFound();
        return Ok(ContainerResponse.From(container));
    }

    [HttpGet("utilization")]
    public async Task<ActionResult<UtilizationResponse>> Utilization(CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanManageContainers(Role!))
            return Forbid();

        return Ok(await containerService.GetUtilizationAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<ContainerResponse>> Register(
        [FromBody] RegisterContainerRequest request, CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanManageContainers(Role!))
            return Forbid();

        try
        {
            var container = await containerService.RegisterAsync(request, ct);
            logger.LogInformation("Container {ContainerNumber} registered.", container.ContainerNumber);
            return CreatedAtAction(nameof(GetById), new { id = container.Id }, ContainerResponse.From(container));
        }
        catch (InvalidContainerTransitionException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/allocate")]
    public async Task<ActionResult<ContainerResponse>> Allocate(
        Guid id, [FromBody] AllocateContainerRequest request, CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanManageContainers(Role!))
            return Forbid();

        try
        {
            var container = await containerService.AllocateAsync(id, request, CorrelationId, ct);
            return Ok(ContainerResponse.From(container));
        }
        catch (InvalidContainerTransitionException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/move")]
    public async Task<ActionResult<ContainerResponse>> Move(
        Guid id, [FromBody] MoveContainerRequest request, CancellationToken ct)
    {
        if (!AuthorizationPolicies.CanManageContainers(Role!))
            return Forbid();

        try
        {
            var container = await containerService.MoveAsync(id, request, CorrelationId, ct);
            return Ok(ContainerResponse.From(container));
        }
        catch (InvalidContainerTransitionException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}