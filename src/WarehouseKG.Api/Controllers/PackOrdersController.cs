using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.PackOrders.Commands;
using WarehouseKG.Application.Features.PackOrders.Dtos;
using WarehouseKG.Application.Features.PackOrders.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages pack orders (packing) for the current tenant.
/// </summary>
[Route("api/v1/pack-orders")]
public class PackOrdersController : ApiControllerBase
{
    private readonly ISender _sender;

    public PackOrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all pack orders.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PackOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PackOrderSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPackOrdersQuery(), cancellationToken));

    /// <summary>Returns a single pack order by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PackOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPackOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft pack order.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreatePackOrderCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Completes a draft pack order.</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CompletePackOrderCommand(id), cancellationToken));

    /// <summary>Cancels a draft pack order.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelPackOrderCommand(id), cancellationToken));
}
