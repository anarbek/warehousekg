using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.StockAudits.Commands;
using WarehouseKG.Application.Features.StockAudits.Dtos;
using WarehouseKG.Application.Features.StockAudits.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages stock audits (physical counts / stocktakes) for the current tenant.
/// </summary>
[Route("api/v1/stock-audits")]
public class StockAuditsController : ApiControllerBase
{
    private readonly ISender _sender;

    public StockAuditsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all stock audits.</summary>
    [HttpGet]
    [Authorize(Policy = "stock-audits:read")]
    [ProducesResponseType(typeof(IReadOnlyList<StockAuditSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockAuditSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetStockAuditsQuery(), cancellationToken));

    /// <summary>Returns a single stock audit by id, including per-line variance.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "stock-audits:read")]
    [ProducesResponseType(typeof(StockAuditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockAuditDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockAuditByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft stock audit, snapshotting current on-hand quantities.</summary>
    [HttpPost]
    [Authorize(Policy = "stock-audits:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateStockAuditCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Completes a draft audit, reconciling stock on hand to the counted quantities.</summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "stock-audits:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CompleteStockAuditCommand(id), cancellationToken));

    /// <summary>Cancels a draft audit.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "stock-audits:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelStockAuditCommand(id), cancellationToken));

    /// <summary>Deletes a stock audit permanently.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "stock-audits:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteStockAuditCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
