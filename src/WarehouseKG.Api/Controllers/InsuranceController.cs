using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Vehicles.Insurance.Commands;
using WarehouseKG.Application.Features.Vehicles.Insurance.Queries;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/vehicles/{vehicleId:guid}/insurance")]
[Produces("application/json")]
public class InsuranceController : ControllerBase
{
    private readonly ISender _sender;
    public InsuranceController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<InsuranceRecordDto>>> GetAll(Guid vehicleId, CancellationToken ct)
        => Ok(await _sender.Send(new GetInsuranceRecordsQuery(vehicleId), ct));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create(Guid vehicleId, CreateInsuranceRecordRequest request, CancellationToken ct)
    {
        var cmd = new CreateInsuranceRecordCommand(
            vehicleId, request.PolicyNumber, request.Provider, request.CoverageType,
            request.StartDate, request.EndDate, request.PremiumAmount, request.Description);
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetAll), new { vehicleId }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid vehicleId, Guid id, UpdateInsuranceRecordRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateInsuranceRecordCommand(
            id, request.PolicyNumber, request.Provider, request.CoverageType,
            request.StartDate, request.EndDate, request.PremiumAmount, request.Description), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteInsuranceRecordCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateInsuranceRecordRequest(
    string PolicyNumber, string Provider, string? CoverageType,
    DateTime StartDate, DateTime EndDate, decimal PremiumAmount,
    string? Description);

public record UpdateInsuranceRecordRequest(
    string PolicyNumber, string Provider, string? CoverageType,
    DateTime StartDate, DateTime EndDate, decimal PremiumAmount,
    string? Description);
