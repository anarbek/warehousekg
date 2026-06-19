using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.Vehicles.VehicleTypes.Commands;

public record CreateVehicleTypeCommand(
    string Code, string Name, string? Description,
    decimal? DefaultCapacityKg, decimal? DefaultCapacityM3,
    bool IsActive = true) : IRequest<Guid>;

public class CreateVehicleTypeCommandHandler : IRequestHandler<CreateVehicleTypeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateVehicleTypeCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateVehicleTypeCommand r, CancellationToken ct)
    {
        var e = new VehicleType
        {
            Id = Guid.NewGuid(), Code = r.Code, Name = r.Name,
            Description = r.Description,
            DefaultCapacityKg = r.DefaultCapacityKg,
            DefaultCapacityM3 = r.DefaultCapacityM3,
            IsActive = r.IsActive
        };
        _context.VehicleTypes.Add(e);
        await _context.SaveChangesAsync(ct);
        return e.Id;
    }
}
