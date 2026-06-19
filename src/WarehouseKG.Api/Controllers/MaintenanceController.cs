using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Vehicles.Maintenance.Commands;
using WarehouseKG.Application.Features.Vehicles.Maintenance.Queries;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/vehicles/{vehicleId:guid}/maintenance")]
[Produces("application/json")]
public class MaintenanceController : ControllerBase
{
    private readonly ISender _sender;
    public MaintenanceController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<MaintenanceRecordDto>>> GetAll(Guid vehicleId, CancellationToken ct)
        => Ok(await _sender.Send(new GetMaintenanceRecordsQuery(vehicleId), ct));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create(Guid vehicleId, CreateMaintenanceRecordRequest request, CancellationToken ct)
    {
        var cmd = new CreateMaintenanceRecordCommand(
            vehicleId, request.MaintenanceType, request.Date, request.MileageKm,
            request.Cost, request.Description, request.ServiceProvider, request.Notes,
            request.NextDueMileageKm, request.NextDueDate);
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetAll), new { vehicleId }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid vehicleId, Guid id, UpdateMaintenanceRecordRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateMaintenanceRecordCommand(
            id, request.MaintenanceType, request.Date, request.MileageKm,
            request.Cost, request.Description, request.ServiceProvider, request.Notes,
            request.NextDueMileageKm, request.NextDueDate), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteMaintenanceRecordCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateMaintenanceRecordRequest(
    MaintenanceType MaintenanceType, DateTime Date, decimal MileageKm,
    decimal Cost, string? Description, string? ServiceProvider, string? Notes,
    decimal? NextDueMileageKm, DateTime? NextDueDate);

public record UpdateMaintenanceRecordRequest(
    MaintenanceType MaintenanceType, DateTime Date, decimal MileageKm,
    decimal Cost, string? Description, string? ServiceProvider, string? Notes,
    decimal? NextDueMileageKm, DateTime? NextDueDate);
