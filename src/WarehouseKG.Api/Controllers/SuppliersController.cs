using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.Suppliers.Commands;
using WarehouseKG.Application.Features.Suppliers.Dtos;
using WarehouseKG.Application.Features.Suppliers.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages suppliers for the current tenant.
/// </summary>
[ApiController]
[Route("api/v1/suppliers")]
[Produces("application/json")]
public class SuppliersController : ControllerBase
{
    private readonly ISender _sender;

    public SuppliersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all suppliers.</summary>
    [HttpGet]
    [Authorize(Policy = "suppliers:read")]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SupplierDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetSuppliersQuery(), cancellationToken));

    /// <summary>Returns a single supplier by id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "suppliers:read")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSupplierByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new supplier.</summary>
    [HttpPost]
    [Authorize(Policy = "suppliers:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateSupplierCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Updates an existing supplier.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "suppliers:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSupplierCommand(
            id,
            request.Code,
            request.Name,
            request.ContactName,
            request.Email,
            request.Phone,
            request.Address,
            request.TaxId,
            request.IsActive);

        var updated = await _sender.Send(command, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a supplier.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "suppliers:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteSupplierCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

/// <summary>Request body for updating a supplier.</summary>
public record UpdateSupplierRequest(
    string Code,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? TaxId,
    bool IsActive);
