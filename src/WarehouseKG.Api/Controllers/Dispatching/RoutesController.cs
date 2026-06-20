using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Dispatching.Routes.Commands;
using WarehouseKG.Application.Features.Dispatching.Routes.Queries;

namespace WarehouseKG.Api.Controllers.Dispatching;

[ApiController]
[Route("api/v1/routes")]
[Produces("application/json")]
public class RoutesController : ApiControllerBase
{
    private readonly ISender _sender;

    public RoutesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "delivery-routes:read")]
    [ProducesResponseType(typeof(IReadOnlyList<RouteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteDto>>> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetDeliveryRoutesQuery(), ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "delivery-routes:read")]
    [ProducesResponseType(typeof(RouteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetDeliveryRouteByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("{id:guid}/detail")]
    [Authorize(Policy = "delivery-routes:read")]
    [ProducesResponseType(typeof(RouteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteDetailDto>> GetDetail(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetDeliveryRouteDetailQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize(Policy = "delivery-routes:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateDeliveryRouteCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "delivery-routes:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateDeliveryRouteRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateDeliveryRouteCommand(
            id, request.Code, request.Date, request.VehicleId,
            request.DriverEmployeeId, request.Notes), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "delivery-routes:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteDeliveryRouteCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = "delivery-routes:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
        => MapWorkflowResult(await _sender.Send(new StartDeliveryRouteCommand(id), ct));

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "delivery-routes:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
        => MapWorkflowResult(await _sender.Send(new CompleteDeliveryRouteCommand(id), ct));

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "delivery-routes:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        => MapWorkflowResult(await _sender.Send(new CancelDeliveryRouteCommand(id), ct));

    // ─── Driver mobile endpoints ───────────────────

    /// <summary>Returns routes assigned to the currently authenticated driver (Planned/InProgress only).</summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<RouteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteDto>>> GetMyRoutes(CancellationToken ct)
        => Ok(await _sender.Send(new GetMyRoutesQuery(), ct));

    /// <summary>Returns full route detail with stops and shipments for the driver's route.</summary>
    [HttpGet("my/{id:guid}/detail")]
    [Authorize]
    [ProducesResponseType(typeof(RouteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteDetailDto>> GetMyRouteDetail(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetMyRouteDetailQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }
}

public record UpdateDeliveryRouteRequest(
    string Code,
    DateTime Date,
    Guid? VehicleId,
    Guid? DriverEmployeeId,
    string? Notes);
