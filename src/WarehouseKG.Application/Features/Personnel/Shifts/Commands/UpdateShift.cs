using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Shifts.Commands;

public record UpdateShiftCommand(Guid Id, string Code, string Name, TimeOnly StartTime, TimeOnly EndTime, bool IsActive) : IRequest<bool>;

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateShiftCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateShiftCommand r, CancellationToken ct)
    {
        var s = await _context.Shifts.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (s is null) return false;
        s.Code = r.Code; s.Name = r.Name; s.StartTime = r.StartTime; s.EndTime = r.EndTime; s.IsActive = r.IsActive;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
