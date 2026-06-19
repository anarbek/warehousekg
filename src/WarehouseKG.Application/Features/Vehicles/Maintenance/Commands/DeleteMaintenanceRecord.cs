using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.Maintenance.Commands;

public record DeleteMaintenanceRecordCommand(Guid Id) : IRequest<bool>;

public class DeleteMaintenanceRecordCommandHandler : IRequestHandler<DeleteMaintenanceRecordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteMaintenanceRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteMaintenanceRecordCommand r, CancellationToken ct)
    {
        var m = await _context.VehicleMaintenanceRecords.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (m is null) return false;
        _context.VehicleMaintenanceRecords.Remove(m);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
