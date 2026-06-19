using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Vehicles.DriverAssignments.Commands;
using WarehouseKG.Application.Features.Vehicles.DriverAssignments.Queries;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/vehicles/{vehicleId:guid}/assignments")]
[Produces("application/json")]
public class DriverAssignmentsController : ControllerBase
{
    private readonly ISender _sender;
    public DriverAssignmentsController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<DriverAssignmentDto>>> GetByVehicle(Guid vehicleId, CancellationToken ct)
        => Ok(await _sender.Send(new GetAssignmentsByVehicleQuery(vehicleId), ct));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create(Guid vehicleId, CreateDriverAssignmentRequest request, CancellationToken ct)
    {
        var cmd = new CreateDriverAssignmentCommand(
            vehicleId, request.EmployeeId, request.AssignedFromUtc,
            request.AssignedToUtc, request.IsPrimary);
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetByVehicle), new { vehicleId }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid vehicleId, Guid id, UpdateDriverAssignmentRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateDriverAssignmentCommand(
            id, request.AssignedFromUtc, request.AssignedToUtc, request.IsPrimary), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteDriverAssignmentCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateDriverAssignmentRequest(
    Guid EmployeeId, DateTime AssignedFromUtc,
    DateTime? AssignedToUtc, bool IsPrimary);

public record UpdateDriverAssignmentRequest(
    DateTime AssignedFromUtc, DateTime? AssignedToUtc, bool IsPrimary);
