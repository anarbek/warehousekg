using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.StockTransfers.Commands;
using WarehouseKG.Application.Features.StockTransfers.Dtos;
using WarehouseKG.Application.Features.StockTransfers.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages stock transfers between warehouses for the current tenant.
/// </summary>
[Route("api/v1/stock-transfers")]
public class StockTransfersController : ApiControllerBase
{
    private readonly ISender _sender;

    public StockTransfersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all stock transfers.</summary>
    [HttpGet]
    [Authorize(Policy = "stock-transfers:read")]
    [ProducesResponseType(typeof(IReadOnlyList<StockTransferSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockTransferSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetStockTransfersQuery(), cancellationToken));

    /// <summary>Returns a single stock transfer by id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "stock-transfers:read")]
    [ProducesResponseType(typeof(StockTransferDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockTransferDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockTransferByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft stock transfer.</summary>
    [HttpPost]
    [Authorize(Policy = "stock-transfers:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateStockTransferCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Completes a draft transfer.</summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "stock-transfers:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CompleteStockTransferCommand(id), cancellationToken));

    /// <summary>Cancels a draft transfer.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "stock-transfers:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelStockTransferCommand(id), cancellationToken));
}
