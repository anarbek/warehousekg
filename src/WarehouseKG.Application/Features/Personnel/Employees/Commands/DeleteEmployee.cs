using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Employees.Commands;

public record DeleteEmployeeCommand(Guid Id) : IRequest<bool>;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteEmployeeCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteEmployeeCommand r, CancellationToken ct)
    {
        var e = await _context.Employees.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (e is null) return false;
        _context.Employees.Remove(e);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
