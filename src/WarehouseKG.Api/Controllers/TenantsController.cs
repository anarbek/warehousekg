using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Tenants.Commands;
using WarehouseKG.Application.Tenants.DTOs;
using WarehouseKG.Application.Tenants.Queries;
using WarehouseKG.Infrastructure.Identity;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
[Produces("application/json")]
public class TenantsController : ControllerBase
{
    private const string R = "tenants";
    private readonly ISender _sender;
    private readonly UserManager<ApplicationUser> _userManager;

    public TenantsController(ISender sender, UserManager<ApplicationUser> userManager)
    {
        _sender = sender;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Policy = "RequireSuperadmin")]
    [ProducesResponseType(typeof(IReadOnlyList<TenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TenantDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTenantsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireSuperadmin")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTenantByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "RequireSuperadmin")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TenantDto>> Create(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireSuperadmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTenantCommand(id, request.Name, request.Slug, request.ContactEmail,
            request.ContactPhone, request.DefaultCurrency, request.MaxUsers, request.EnabledModules);
        var updated = await _sender.Send(command, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "RequireSuperadmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SuspendTenantCommand(id), cancellationToken);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "RequireSuperadmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateTenantCommand(id), cancellationToken);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("{tenantId:guid}/users/{userId:guid}/password")]
    [Authorize(Policy = "RequireSuperadmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetUserPassword(
        Guid tenantId, Guid userId, ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.TenantId != tenantId)
            return NotFound();

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return BadRequest(removeResult.Errors.Select(e => e.Description));

        var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addResult.Succeeded)
            return BadRequest(addResult.Errors.Select(e => e.Description));

        return NoContent();
    }
}

public record UpdateTenantRequest(
    string Name,
    string Slug,
    string? ContactEmail,
    string? ContactPhone,
    string DefaultCurrency,
    int? MaxUsers,
    string? EnabledModules);

public record ResetPasswordRequest(string NewPassword);
