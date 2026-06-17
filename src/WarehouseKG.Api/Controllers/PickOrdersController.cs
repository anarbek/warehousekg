using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.PickOrders.Commands;
using WarehouseKG.Application.Features.PickOrders.Dtos;
using WarehouseKG.Application.Features.PickOrders.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages pick orders (picking) for the current tenant.
/// </summary>
[Route("api/v1/pick-orders")]
public class PickOrdersController : ApiControllerBase
{
    private readonly ISender _sender;

    public PickOrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all pick orders.</summary>
    [HttpGet]
    [Authorize(Policy = "pick-orders:read")]
    [ProducesResponseType(typeof(IReadOnlyList<PickOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PickOrderSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPickOrdersQuery(), cancellationToken));

    /// <summary>Returns a single pick order by id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "pick-orders:read")]
    [ProducesResponseType(typeof(PickOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PickOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPickOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft pick order.</summary>
    [HttpPost]
    [Authorize(Policy = "pick-orders:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePickOrderCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Completes a draft pick order and decreases stock on hand.</summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "pick-orders:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CompletePickOrderCommand(id), cancellationToken));

    /// <summary>Cancels a draft pick order.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "pick-orders:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelPickOrderCommand(id), cancellationToken));
}
