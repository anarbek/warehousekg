using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Dispatching.Geofences.Commands;
using WarehouseKG.Application.Features.Dispatching.Geofences.Queries;

namespace WarehouseKG.Api.Controllers.Dispatching;

[ApiController]
[Route("api/v1/geofences")]
[Produces("application/json")]
public class GeofencesController : ApiControllerBase
{
    private readonly ISender _sender;

    public GeofencesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "geofences:read")]
    [ProducesResponseType(typeof(IReadOnlyList<GeofenceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GeofenceDto>>> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetGeofencesQuery(), ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "geofences:read")]
    [ProducesResponseType(typeof(GeofenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeofenceDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetGeofenceByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("check/{stopId:guid}")]
    [Authorize(Policy = "geofences:read")]
    [ProducesResponseType(typeof(IReadOnlyList<GeofenceCheckResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GeofenceCheckResultDto>>> CheckStop(
        Guid stopId, CancellationToken ct)
        => Ok(await _sender.Send(new CheckStopAgainstGeofencesQuery(stopId), ct));

    [HttpPost]
    [Authorize(Policy = "geofences:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateGeofenceCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "geofences:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateGeofenceRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateGeofenceCommand(
            id, request.Code, request.Name, request.Type,
            request.Vertices, request.IsActive), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "geofences:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteGeofenceCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdateGeofenceRequest(
    string Code,
    string Name,
    Domain.Enums.GeofenceType Type,
    List<GeoPointInput> Vertices,
    bool IsActive);
