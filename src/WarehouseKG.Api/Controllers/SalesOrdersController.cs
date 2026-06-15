using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.SalesOrders.Commands;
using WarehouseKG.Application.Features.SalesOrders.Dtos;
using WarehouseKG.Application.Features.SalesOrders.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages sales orders for the current tenant.
/// </summary>
[Route("api/v1/sales-orders")]
public class SalesOrdersController : ApiControllerBase
{
    private readonly ISender _sender;

    public SalesOrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all sales orders.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SalesOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SalesOrderSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetSalesOrdersQuery(), cancellationToken));

    /// <summary>Returns a single sales order by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SalesOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalesOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSalesOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft sales order.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateSalesOrderCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Confirms a draft sales order.</summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new ConfirmSalesOrderCommand(id), cancellationToken));

    /// <summary>Ships a confirmed sales order and decreases stock on hand.</summary>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Ship(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new ShipSalesOrderCommand(id), cancellationToken));

    /// <summary>Cancels a draft or confirmed sales order.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelSalesOrderCommand(id), cancellationToken));
}
