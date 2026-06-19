using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Personnel.Positions.Commands;
using WarehouseKG.Application.Features.Personnel.Positions.Dtos;
using WarehouseKG.Application.Features.Personnel.Positions.Queries;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/positions")]
[Produces("application/json")]
public class PositionsController : ControllerBase
{
    private readonly ISender _sender;
    public PositionsController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize(Policy = "positions:read")]
    public async Task<ActionResult<IReadOnlyList<PositionDto>>> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetPositionsQuery(), ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "positions:read")]
    public async Task<ActionResult<PositionDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetPositionByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize(Policy = "positions:write")]
    public async Task<ActionResult<Guid>> Create(CreatePositionCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "positions:write")]
    public async Task<IActionResult> Update(Guid id, UpdatePositionRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdatePositionCommand(id, request.Code, request.Name, request.Description, request.IsActive), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "positions:delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeletePositionCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdatePositionRequest(string Code, string Name, string? Description, bool IsActive);
