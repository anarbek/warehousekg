using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.Warehouses.Commands;
using WarehouseKG.Application.Features.Warehouses.Dtos;
using WarehouseKG.Application.Features.Warehouses.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages warehouses for the current tenant.
/// </summary>
[ApiController]
[Route("api/v1/warehouses")]
[Produces("application/json")]
public class WarehousesController : ControllerBase
{
    private const string R = "warehouses";

    private readonly ISender _sender;

    public WarehousesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all warehouses for the current tenant.</summary>
    [HttpGet]
    [Authorize(Policy = R + ":read")]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWarehousesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single warehouse by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = R + ":read")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWarehouseByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new warehouse.</summary>
    [HttpPost]
    [Authorize(Policy = R + ":write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(
        CreateWarehouseCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Updates an existing warehouse.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = R + ":write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWarehouseCommand(
            id, request.Code, request.Name, request.Address, request.IsActive);

        var updated = await _sender.Send(command, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a warehouse.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = R + ":delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteWarehouseCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

/// <summary>Request body for updating a warehouse.</summary>
public record UpdateWarehouseRequest(
    string Code,
    string Name,
    string? Address,
    bool IsActive);
