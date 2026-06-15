using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.PurchaseOrders.Commands;
using WarehouseKG.Application.Features.PurchaseOrders.Dtos;
using WarehouseKG.Application.Features.PurchaseOrders.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages purchase orders for the current tenant.
/// </summary>
[Route("api/v1/purchase-orders")]
public class PurchaseOrdersController : ApiControllerBase
{
    private readonly ISender _sender;

    public PurchaseOrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all purchase orders.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PurchaseOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PurchaseOrderSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPurchaseOrdersQuery(), cancellationToken));

    /// <summary>Returns a single purchase order by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PurchaseOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPurchaseOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft purchase order.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreatePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Submits a draft purchase order to the supplier.</summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new SubmitPurchaseOrderCommand(id), cancellationToken));

    /// <summary>Marks a submitted purchase order as received and increases stock on hand.</summary>
    [HttpPost("{id:guid}/receive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Receive(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new ReceivePurchaseOrderCommand(id), cancellationToken));

    /// <summary>Cancels a draft or submitted purchase order.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelPurchaseOrderCommand(id), cancellationToken));
}
