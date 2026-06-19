using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Vehicles.Inspections.Commands;
using WarehouseKG.Application.Features.Vehicles.Inspections.Queries;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/vehicles/{vehicleId:guid}/inspections")]
[Produces("application/json")]
public class InspectionsController : ControllerBase
{
    private readonly ISender _sender;
    public InspectionsController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<InspectionRecordDto>>> GetAll(Guid vehicleId, CancellationToken ct)
        => Ok(await _sender.Send(new GetInspectionRecordsQuery(vehicleId), ct));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create(Guid vehicleId, CreateInspectionRecordRequest request, CancellationToken ct)
    {
        var cmd = new CreateInspectionRecordCommand(
            vehicleId, request.InspectionDate, request.ExpiryDate,
            request.Result, request.Inspector, request.Notes);
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetAll), new { vehicleId }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid vehicleId, Guid id, UpdateInspectionRecordRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateInspectionRecordCommand(
            id, request.InspectionDate, request.ExpiryDate,
            request.Result, request.Inspector, request.Notes), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteInspectionRecordCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateInspectionRecordRequest(
    DateTime InspectionDate, DateTime ExpiryDate,
    InspectionResult Result, string? Inspector, string? Notes);

public record UpdateInspectionRecordRequest(
    DateTime InspectionDate, DateTime ExpiryDate,
    InspectionResult Result, string? Inspector, string? Notes);
