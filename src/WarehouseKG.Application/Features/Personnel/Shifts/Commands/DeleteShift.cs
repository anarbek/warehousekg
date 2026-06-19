using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Shifts.Commands;

public record DeleteShiftCommand(Guid Id) : IRequest<bool>;

public class DeleteShiftCommandHandler : IRequestHandler<DeleteShiftCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteShiftCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteShiftCommand r, CancellationToken ct)
    {
        var s = await _context.Shifts.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (s is null) return false;
        _context.Shifts.Remove(s);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
