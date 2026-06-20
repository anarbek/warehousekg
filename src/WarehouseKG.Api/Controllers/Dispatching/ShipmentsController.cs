using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Features.Dispatching.Shipments.Commands;
using WarehouseKG.Application.Features.Dispatching.Shipments.Queries;
using ShipmentDto = WarehouseKG.Application.Features.Dispatching.Routes.Queries.ShipmentDto;

namespace WarehouseKG.Api.Controllers.Dispatching;

[ApiController]
[Route("api/v1/routes/{routeId}/stops/{stopId}/shipments")]
[Produces("application/json")]
public class ShipmentsController : ApiControllerBase
{
    private readonly ISender _sender;

    public ShipmentsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "delivery-shipments:read")]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ShipmentDto>>> GetAll(Guid stopId, CancellationToken ct)
        => Ok(await _sender.Send(new GetShipmentsByStopQuery(stopId), ct));

    [HttpPost]
    [Authorize(Policy = "delivery-shipments:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Assign(Guid stopId, AssignShipmentRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new AssignShipmentCommand(stopId, request.SalesOrderId), ct);
        return MapWorkflowResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "delivery-shipments:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveShipmentCommand(id), ct);
        return MapWorkflowResult(result);
    }
}

public record AssignShipmentRequest(Guid SalesOrderId);
