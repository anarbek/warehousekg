using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.VehicleTypes.Queries;

public record GetVehicleTypesQuery : IRequest<IReadOnlyList<VehicleTypeDto>>;

public class GetVehicleTypesQueryHandler : IRequestHandler<GetVehicleTypesQuery, IReadOnlyList<VehicleTypeDto>>
{
    private readonly IApplicationDbContext _context;
    public GetVehicleTypesQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<VehicleTypeDto>> Handle(GetVehicleTypesQuery r, CancellationToken ct)
        => await _context.VehicleTypes.AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new VehicleTypeDto
            {
                Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description,
                DefaultCapacityKg = x.DefaultCapacityKg, DefaultCapacityM3 = x.DefaultCapacityM3,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
}

public record GetVehicleTypeByIdQuery(Guid Id) : IRequest<VehicleTypeDto?>;

public class GetVehicleTypeByIdQueryHandler : IRequestHandler<GetVehicleTypeByIdQuery, VehicleTypeDto?>
{
    private readonly IApplicationDbContext _context;
    public GetVehicleTypeByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<VehicleTypeDto?> Handle(GetVehicleTypeByIdQuery r, CancellationToken ct)
        => await _context.VehicleTypes.AsNoTracking().Where(x => x.Id == r.Id)
            .Select(x => new VehicleTypeDto
            {
                Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description,
                DefaultCapacityKg = x.DefaultCapacityKg, DefaultCapacityM3 = x.DefaultCapacityM3,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
}

public class VehicleTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DefaultCapacityKg { get; set; }
    public decimal? DefaultCapacityM3 { get; set; }
    public bool IsActive { get; set; }
}
