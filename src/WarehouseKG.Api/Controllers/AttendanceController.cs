using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Personnel.Attendance.Commands;
using WarehouseKG.Application.Features.Personnel.Attendance.Dtos;
using WarehouseKG.Application.Features.Personnel.Attendance.Queries;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/attendance")]
[Produces("application/json")]
public class AttendanceController : ControllerBase
{
    private readonly ISender _sender;
    public AttendanceController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize(Policy = "attendance:read")]
    public async Task<ActionResult<IReadOnlyList<AttendanceDto>>> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _sender.Send(new GetAttendanceQuery(from, to), ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "attendance:read")]
    public async Task<ActionResult<AttendanceDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetAttendanceByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize(Policy = "attendance:write")]
    public async Task<ActionResult<Guid>> Create(CreateAttendanceCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "attendance:write")]
    public async Task<IActionResult> Update(Guid id, UpdateAttendanceCommand command, CancellationToken ct)
    {
        var updated = await _sender.Send(command with { Id = id }, ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "attendance:delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteAttendanceCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}
