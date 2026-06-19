using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Departments.Commands;

public record DeleteDepartmentCommand(Guid Id) : IRequest<bool>;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteDepartmentCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteDepartmentCommand r, CancellationToken ct)
    {
        var d = await _context.Departments.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (d is null) return false;
        _context.Departments.Remove(d);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
