using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Personnel.Shifts.Commands;
using WarehouseKG.Application.Features.Personnel.Shifts.Dtos;
using WarehouseKG.Application.Features.Personnel.Shifts.Queries;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/shifts")]
[Produces("application/json")]
public class ShiftsController : ControllerBase
{
    private readonly ISender _sender;
    public ShiftsController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize(Policy = "shifts:read")]
    public async Task<ActionResult<IReadOnlyList<ShiftDto>>> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetShiftsQuery(), ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "shifts:read")]
    public async Task<ActionResult<ShiftDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetShiftByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize(Policy = "shifts:write")]
    public async Task<ActionResult<Guid>> Create(CreateShiftCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "shifts:write")]
    public async Task<IActionResult> Update(Guid id, UpdateShiftRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateShiftCommand(id, request.Code, request.Name, request.StartTime, request.EndTime, request.IsActive), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "shifts:delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteShiftCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdateShiftRequest(string Code, string Name, TimeOnly StartTime, TimeOnly EndTime, bool IsActive);
