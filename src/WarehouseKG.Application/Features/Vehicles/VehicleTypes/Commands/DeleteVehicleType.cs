using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.VehicleTypes.Commands;

public record DeleteVehicleTypeCommand(Guid Id) : IRequest<bool>;

public class DeleteVehicleTypeCommandHandler : IRequestHandler<DeleteVehicleTypeCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteVehicleTypeCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteVehicleTypeCommand r, CancellationToken ct)
    {
        var e = await _context.VehicleTypes.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (e is null) return false;
        _context.VehicleTypes.Remove(e);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
