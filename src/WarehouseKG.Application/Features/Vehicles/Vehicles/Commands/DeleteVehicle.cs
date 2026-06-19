using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.Vehicles.Commands;

public record DeleteVehicleCommand(Guid Id) : IRequest<bool>;

public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteVehicleCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteVehicleCommand r, CancellationToken ct)
    {
        var v = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (v is null) return false;
        _context.Vehicles.Remove(v);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
