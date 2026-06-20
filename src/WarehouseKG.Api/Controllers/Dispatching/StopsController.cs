using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Dispatching.Routes.Queries;
using WarehouseKG.Application.Features.Dispatching.Stops.Commands;
using WarehouseKG.Application.Features.Dispatching.Stops.Queries;

namespace WarehouseKG.Api.Controllers.Dispatching;

[ApiController]
[Route("api/v1/routes/{routeId}/stops")]
[Produces("application/json")]
public class StopsController : ApiControllerBase
{
    private readonly ISender _sender;

    public StopsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "delivery-stops:read")]
    [ProducesResponseType(typeof(IReadOnlyList<StopDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StopDto>>> GetAll(Guid routeId, CancellationToken ct)
        => Ok(await _sender.Send(new GetStopsByRouteQuery(routeId), ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "delivery-stops:read")]
    [ProducesResponseType(typeof(StopDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StopDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetDeliveryStopByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize(Policy = "delivery-stops:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(Guid routeId, CreateDeliveryStopCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { routeId, id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "delivery-stops:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateDeliveryStopRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateDeliveryStopCommand(
            id, request.SequenceNumber, request.CustomerId, request.Address,
            request.Latitude, request.Longitude,
            request.PlannedArrivalUtc, request.PlannedDepartureUtc,
            request.Notes), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "delivery-stops:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteDeliveryStopCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/arrive")]
    [Authorize(Policy = "delivery-stops:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Arrive(Guid id, CancellationToken ct)
        => MapWorkflowResult(await _sender.Send(new ArriveAtStopCommand(id), ct));

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "delivery-stops:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
        => MapWorkflowResult(await _sender.Send(new CompleteDeliveryStopCommand(id), ct));

    [HttpPost("{id:guid}/skip")]
    [Authorize(Policy = "delivery-stops:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Skip(Guid id, CancellationToken ct)
        => MapWorkflowResult(await _sender.Send(new SkipDeliveryStopCommand(id), ct));
}

public record UpdateDeliveryStopRequest(
    int SequenceNumber,
    Guid? CustomerId,
    string Address,
    double? Latitude,
    double? Longitude,
    DateTime? PlannedArrivalUtc,
    DateTime? PlannedDepartureUtc,
    string? Notes);
