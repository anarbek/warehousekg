using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Departments.Commands;

public record UpdateDepartmentCommand(Guid Id, string Code, string Name, string? Description, bool IsActive) : IRequest<bool>;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateDepartmentCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateDepartmentCommand r, CancellationToken ct)
    {
        var d = await _context.Departments.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (d is null) return false;
        d.Code = r.Code; d.Name = r.Name; d.Description = r.Description; d.IsActive = r.IsActive;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
