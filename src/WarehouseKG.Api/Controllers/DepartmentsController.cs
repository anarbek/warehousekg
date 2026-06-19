using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Personnel.Departments.Commands;
using WarehouseKG.Application.Features.Personnel.Departments.Dtos;
using WarehouseKG.Application.Features.Personnel.Departments.Queries;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/departments")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly ISender _sender;
    public DepartmentsController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize(Policy = "departments:read")]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetDepartmentsQuery(), ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "departments:read")]
    public async Task<ActionResult<DepartmentDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetDepartmentByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize(Policy = "departments:write")]
    public async Task<ActionResult<Guid>> Create(CreateDepartmentCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "departments:write")]
    public async Task<IActionResult> Update(Guid id, UpdateDepartmentRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateDepartmentCommand(id, request.Code, request.Name, request.Description, request.IsActive), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "departments:delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteDepartmentCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdateDepartmentRequest(string Code, string Name, string? Description, bool IsActive);
