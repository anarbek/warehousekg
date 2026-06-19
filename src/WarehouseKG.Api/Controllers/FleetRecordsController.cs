using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Vehicles.Maintenance.Queries;
using WarehouseKG.Application.Features.Vehicles.Insurance.Queries;
using WarehouseKG.Application.Features.Vehicles.Inspections.Queries;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class FleetRecordsController : ControllerBase
{
    private readonly ISender _sender;
    public FleetRecordsController(ISender sender) { _sender = sender; }

    [HttpGet("maintenance")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<MaintenanceRecordDto>>> GetAllMaintenance(CancellationToken ct)
        => Ok(await _sender.Send(new GetAllMaintenanceRecordsQuery(), ct));

    [HttpGet("insurance")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<InsuranceRecordDto>>> GetAllInsurance(CancellationToken ct)
        => Ok(await _sender.Send(new GetAllInsuranceRecordsQuery(), ct));

    [HttpGet("inspections")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<InspectionRecordDto>>> GetAllInspections(CancellationToken ct)
        => Ok(await _sender.Send(new GetAllInspectionRecordsQuery(), ct));
}
