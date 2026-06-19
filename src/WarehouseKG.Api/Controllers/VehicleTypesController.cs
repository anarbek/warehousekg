using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Vehicles.VehicleTypes.Commands;
using WarehouseKG.Application.Features.Vehicles.VehicleTypes.Queries;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/vehicle-types")]
[Produces("application/json")]
public class VehicleTypesController : ControllerBase
{
    private readonly ISender _sender;
    public VehicleTypesController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<VehicleTypeDto>>> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetVehicleTypesQuery(), ct));

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<VehicleTypeDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetVehicleTypeByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create(CreateVehicleTypeCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateVehicleTypeRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateVehicleTypeCommand(
            id, request.Code, request.Name, request.Description,
            request.DefaultCapacityKg, request.DefaultCapacityM3, request.IsActive), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteVehicleTypeCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdateVehicleTypeRequest(
    string Code, string Name, string? Description,
    decimal? DefaultCapacityKg, decimal? DefaultCapacityM3, bool IsActive);
