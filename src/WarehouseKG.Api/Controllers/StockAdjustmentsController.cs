using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.StockAdjustments.Commands;
using WarehouseKG.Application.Features.StockAdjustments.Dtos;
using WarehouseKG.Application.Features.StockAdjustments.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages stock adjustments (corrections, damages, write-offs) for the current tenant.
/// </summary>
[Route("api/v1/stock-adjustments")]
public class StockAdjustmentsController : ApiControllerBase
{
    private readonly ISender _sender;

    public StockAdjustmentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all stock adjustments.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StockAdjustmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockAdjustmentSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetStockAdjustmentsQuery(), cancellationToken));

    /// <summary>Returns a single stock adjustment by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StockAdjustmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockAdjustmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockAdjustmentByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft stock adjustment.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateStockAdjustmentCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Completes a draft adjustment, applying each line's change to stock on hand.</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CompleteStockAdjustmentCommand(id), cancellationToken));

    /// <summary>Cancels a draft adjustment.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelStockAdjustmentCommand(id), cancellationToken));
}
