using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Commands;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Queries;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/vehicles")]
[Produces("application/json")]
public class VehiclesController : ControllerBase
{
    private readonly ISender _sender;
    public VehiclesController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<VehicleDto>>> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetVehiclesQuery(), ct));

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetVehicleByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("{id:guid}/detail")]
    [Authorize]
    public async Task<ActionResult<VehicleDetailDto>> GetDetail(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetVehicleDetailQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create(CreateVehicleCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateVehicleRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateVehicleCommand(
            id, request.Code, request.LicensePlate, request.VIN, request.Brand, request.Model,
            request.ManufactureYear, request.VehicleTypeId,
            request.OwnershipType, request.Status, request.FuelType,
            request.FuelConsumptionRate, request.MaxCapacityKg, request.MaxCapacityM3,
            request.CurrentMileageKm, request.PurchaseDate, request.PurchasePrice,
            request.InsurancePolicyNumber, request.InsuranceProvider, request.InsuranceExpiryDate,
            request.TechInspectionExpiryDate,
            request.NextMaintenanceMileageKm, request.NextMaintenanceDate,
            request.HasGpsTracker, request.Notes), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteVehicleCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdateVehicleRequest(
    string Code, string LicensePlate, string? VIN, string Brand, string? Model,
    int? ManufactureYear, Guid? VehicleTypeId,
    VehicleOwnershipType OwnershipType, VehicleStatus Status, FuelType FuelType,
    decimal? FuelConsumptionRate, decimal? MaxCapacityKg, decimal? MaxCapacityM3,
    decimal CurrentMileageKm, DateTime? PurchaseDate, decimal? PurchasePrice,
    string? InsurancePolicyNumber, string? InsuranceProvider, DateTime? InsuranceExpiryDate,
    DateTime? TechInspectionExpiryDate,
    decimal? NextMaintenanceMileageKm, DateTime? NextMaintenanceDate,
    bool HasGpsTracker, string? Notes);
