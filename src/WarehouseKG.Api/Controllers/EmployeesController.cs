using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.Personnel.Employees.Commands;
using WarehouseKG.Application.Features.Personnel.Employees.Dtos;
using WarehouseKG.Application.Features.Personnel.Employees.Queries;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly ISender _sender;
    public EmployeesController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize(Policy = "employees:read")]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetEmployeesQuery(), ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "employees:read")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetEmployeeByIdQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("{id:guid}/detail")]
    [Authorize(Policy = "employees:read")]
    public async Task<ActionResult<EmployeeDetailDto>> GetDetail(Guid id, CancellationToken ct)
    {
        var r = await _sender.Send(new GetEmployeeDetailQuery(id), ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost]
    [Authorize(Policy = "employees:write")]
    public async Task<ActionResult<Guid>> Create(CreateEmployeeCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "employees:write")]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeRequest request, CancellationToken ct)
    {
        var updated = await _sender.Send(new UpdateEmployeeCommand(id, request.Code, request.FirstName, request.LastName,
            request.MiddleName, request.Email, request.Phone, request.HireDate, request.TerminationDate,
            request.PositionId, request.DepartmentId, request.ApplicationUserId, request.IsActive), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "employees:delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _sender.Send(new DeleteEmployeeCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdateEmployeeRequest(
    string Code, string FirstName, string LastName, string? MiddleName,
    string? Email, string? Phone, DateTime? HireDate, DateTime? TerminationDate,
    Guid? PositionId, Guid? DepartmentId, Guid? ApplicationUserId, bool IsActive);
