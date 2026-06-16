using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.StockReceipts.Commands;
using WarehouseKG.Application.Features.StockReceipts.Dtos;
using WarehouseKG.Application.Features.StockReceipts.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages stock receipts (receiving) for the current tenant.
/// </summary>
[Route("api/v1/stock-receipts")]
public class StockReceiptsController : ApiControllerBase
{
    private readonly ISender _sender;

    public StockReceiptsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all stock receipts.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StockReceiptSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockReceiptSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetStockReceiptsQuery(), cancellationToken));

    /// <summary>Returns a single stock receipt by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StockReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockReceiptDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockReceiptByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft stock receipt.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateStockReceiptCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Completes a draft receipt and increases stock on hand.</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CompleteStockReceiptCommand(id), cancellationToken));

    /// <summary>Cancels a draft receipt.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelStockReceiptCommand(id), cancellationToken));
}
