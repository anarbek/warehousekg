using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.PreOrders.Commands;
using WarehouseKG.Application.Features.PreOrders.Dtos;
using WarehouseKG.Application.Features.PreOrders.Queries;

namespace WarehouseKG.Api.Controllers;

[Route("api/v1/pre-orders")]
public class PreOrdersController : ApiControllerBase
{
    private readonly ISender _sender;

    public PreOrdersController(ISender sender) => _sender = sender;

    /// <summary>Returns all pre-orders for the tenant.</summary>
    [HttpGet]
    [Authorize(Policy = "pre-orders:read")]
    [ProducesResponseType(typeof(IReadOnlyList<PreOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PreOrderSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPreOrdersQuery(), cancellationToken));

    /// <summary>Returns the current preseller's own pre-orders.</summary>
    [HttpGet("my")]
    [Authorize(Policy = "pre-orders:write")]
    [ProducesResponseType(typeof(IReadOnlyList<PreOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PreOrderSummaryDto>>> GetMy(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetMyPreOrdersQuery(), cancellationToken));

    /// <summary>Returns a single pre-order with lines.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "pre-orders:read")]
    [ProducesResponseType(typeof(PreOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PreOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPreOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft pre-order.</summary>
    [HttpPost]
    [Authorize(Policy = "pre-orders:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePreOrderCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Updates a draft pre-order.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "pre-orders:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePreOrderCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        return MapWorkflowResult(await _sender.Send(command, cancellationToken));
    }

    /// <summary>Deletes a draft pre-order.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "pre-orders:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new DeletePreOrderCommand(id), cancellationToken));

    /// <summary>Submits a draft pre-order for approval.</summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "pre-orders:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new SubmitPreOrderCommand(id), cancellationToken));

    /// <summary>Approves a submitted pre-order.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "pre-orders:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new ApprovePreOrderCommand(id), cancellationToken));

    /// <summary>Rejects a submitted pre-order with an optional reason.</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "pre-orders:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectPreOrderRequest? body, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new RejectPreOrderCommand(id, body?.Reason), cancellationToken));

    /// <summary>Converts an approved pre-order into a draft sales order.</summary>
    [HttpPost("{id:guid}/convert")]
    [Authorize(Policy = "pre-orders:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Convert(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConvertPreOrderToSalesOrderCommand(id), cancellationToken);
        if (!result.Success)
        {
            return result.Error == "Pre-order not found."
                ? NotFound()
                : Conflict(new { error = result.Error });
        }
        return CreatedAtAction(
            nameof(SalesOrdersController.GetById),
            "SalesOrders",
            new { id = result.SalesOrderId },
            result.SalesOrderId);
    }

    /// <summary>Returns per-item warehouse stock snapshot for pre-order creation.</summary>
    [HttpGet("warehouse-stock")]
    [Authorize(Policy = "warehouses:read")]
    [ProducesResponseType(typeof(IReadOnlyList<PreOrderWarehouseStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PreOrderWarehouseStockDto>>> GetWarehouseStock(
        [FromQuery] Guid warehouseId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPreOrderWarehouseStockQuery(warehouseId), cancellationToken));
}

public record RejectPreOrderRequest(string? Reason);
