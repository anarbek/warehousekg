using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.Personnel.Employees.Commands;

public record CreateEmployeeCommand(
    string Code, string FirstName, string LastName, string? MiddleName,
    string? Email, string? Phone, DateTime? HireDate, DateTime? TerminationDate,
    Guid? PositionId, Guid? DepartmentId, Guid? ApplicationUserId, bool IsActive = true) : IRequest<Guid>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateEmployeeCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateEmployeeCommand r, CancellationToken ct)
    {
        var e = new Employee
        {
            Id = Guid.NewGuid(), Code = r.Code, FirstName = r.FirstName, LastName = r.LastName,
            MiddleName = r.MiddleName, Email = r.Email, Phone = r.Phone,
            HireDate = r.HireDate, TerminationDate = r.TerminationDate,
            PositionId = r.PositionId, DepartmentId = r.DepartmentId,
            ApplicationUserId = r.ApplicationUserId, IsActive = r.IsActive
        };
        _context.Employees.Add(e);
        await _context.SaveChangesAsync(ct);
        return e.Id;
    }
}
