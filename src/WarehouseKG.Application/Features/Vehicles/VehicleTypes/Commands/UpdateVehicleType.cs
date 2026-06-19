using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.VehicleTypes.Commands;

public record UpdateVehicleTypeCommand(
    Guid Id, string Code, string Name, string? Description,
    decimal? DefaultCapacityKg, decimal? DefaultCapacityM3,
    bool IsActive) : IRequest<bool>;

public class UpdateVehicleTypeCommandHandler : IRequestHandler<UpdateVehicleTypeCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateVehicleTypeCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateVehicleTypeCommand r, CancellationToken ct)
    {
        var e = await _context.VehicleTypes.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (e is null) return false;
        e.Code = r.Code; e.Name = r.Name; e.Description = r.Description;
        e.DefaultCapacityKg = r.DefaultCapacityKg; e.DefaultCapacityM3 = r.DefaultCapacityM3;
        e.IsActive = r.IsActive;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
