using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Employees.Commands;

public record UpdateEmployeeCommand(
    Guid Id, string Code, string FirstName, string LastName, string? MiddleName,
    string? Email, string? Phone, DateTime? HireDate, DateTime? TerminationDate,
    Guid? PositionId, Guid? DepartmentId, Guid? ApplicationUserId, bool IsActive) : IRequest<bool>;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateEmployeeCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateEmployeeCommand r, CancellationToken ct)
    {
        var e = await _context.Employees.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (e is null) return false;
        e.Code = r.Code; e.FirstName = r.FirstName; e.LastName = r.LastName; e.MiddleName = r.MiddleName;
        e.Email = r.Email; e.Phone = r.Phone; e.HireDate = r.HireDate; e.TerminationDate = r.TerminationDate;
        e.PositionId = r.PositionId; e.DepartmentId = r.DepartmentId;
        e.ApplicationUserId = r.ApplicationUserId; e.IsActive = r.IsActive;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
