using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.UnitsOfMeasure.Commands;
using WarehouseKG.Application.Features.UnitsOfMeasure.Dtos;
using WarehouseKG.Application.Features.UnitsOfMeasure.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages units of measure for the current tenant.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.RequireManager)]
[ApiController]
[Route("api/v1/units-of-measure")]
[Produces("application/json")]
public class UnitsOfMeasureController : ControllerBase
{
    private readonly ISender _sender;

    public UnitsOfMeasureController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all units of measure.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UnitOfMeasureDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UnitOfMeasureDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetUnitsOfMeasureQuery(), cancellationToken));

    /// <summary>Returns a single unit by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UnitOfMeasureDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnitOfMeasureDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUnitOfMeasureByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new unit of measure.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateUnitOfMeasureCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Updates an existing unit of measure.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateUnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUnitOfMeasureCommand(id, request.Code, request.Name, request.Description, request.IsActive);
        var updated = await _sender.Send(command, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a unit of measure.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteUnitOfMeasureCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

/// <summary>Request body for updating a unit of measure.</summary>
public record UpdateUnitOfMeasureRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive);
