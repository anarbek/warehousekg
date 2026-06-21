using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.InventoryItems.Commands;
using WarehouseKG.Application.Features.InventoryItems.Dtos;
using WarehouseKG.Application.Features.InventoryItems.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages inventory items for the current tenant.
/// </summary>
[ApiController]
[Route("api/v1/inventory-items")]
[Produces("application/json")]
public class InventoryItemsController : ControllerBase
{
    private readonly ISender _sender;

    public InventoryItemsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all inventory items for the current tenant.</summary>
    [HttpGet]
    [Authorize(Policy = "inventory-items:read")]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInventoryItemsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single inventory item by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "inventory-items:read")]
    [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryItemDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInventoryItemByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new inventory item.</summary>
    [HttpPost]
    [Authorize(Policy = "inventory-items:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(
        CreateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Updates an existing inventory item.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "inventory-items:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateInventoryItemCommand(
            id,
            request.Sku,
            request.Name,
            request.Description,
            request.Barcode,
            request.CategoryId,
            request.UnitOfMeasureId,
            request.ReorderLevel,
            request.UnitPrice,
            request.IsActive);

        var updated = await _sender.Send(command, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes an inventory item.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "inventory-items:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteInventoryItemCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

/// <summary>Request body for updating an inventory item.</summary>
public record UpdateInventoryItemRequest(
    string Sku,
    string Name,
    string? Description,
    string? Barcode,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    decimal ReorderLevel,
    decimal UnitPrice,
    bool IsActive);
