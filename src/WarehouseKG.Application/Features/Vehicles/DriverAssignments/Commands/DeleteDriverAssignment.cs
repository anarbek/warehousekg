using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.DriverAssignments.Commands;

public record DeleteDriverAssignmentCommand(Guid Id) : IRequest<bool>;

public class DeleteDriverAssignmentCommandHandler : IRequestHandler<DeleteDriverAssignmentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteDriverAssignmentCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteDriverAssignmentCommand r, CancellationToken ct)
    {
        var a = await _context.VehicleDriverAssignments.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (a is null) return false;
        _context.VehicleDriverAssignments.Remove(a);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
