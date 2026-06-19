using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.DriverAssignments.Commands;

public record UpdateDriverAssignmentCommand(
    Guid Id, DateTime AssignedFromUtc, DateTime? AssignedToUtc,
    bool IsPrimary) : IRequest<bool>;

public class UpdateDriverAssignmentCommandHandler : IRequestHandler<UpdateDriverAssignmentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateDriverAssignmentCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateDriverAssignmentCommand r, CancellationToken ct)
    {
        var a = await _context.VehicleDriverAssignments.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (a is null) return false;
        a.AssignedFromUtc = r.AssignedFromUtc; a.AssignedToUtc = r.AssignedToUtc;
        a.IsPrimary = r.IsPrimary;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
