using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Application.Features.Vehicles.DriverAssignments.Queries;

public record GetAssignmentsByVehicleQuery(Guid VehicleId) : IRequest<IReadOnlyList<DriverAssignmentDto>>;

public class GetAssignmentsByVehicleQueryHandler : IRequestHandler<GetAssignmentsByVehicleQuery, IReadOnlyList<DriverAssignmentDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAssignmentsByVehicleQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<DriverAssignmentDto>> Handle(GetAssignmentsByVehicleQuery r, CancellationToken ct)
        => await _context.VehicleDriverAssignments.AsNoTracking()
            .Where(a => a.VehicleId == r.VehicleId).OrderByDescending(a => a.AssignedFromUtc)
            .Select(a => new DriverAssignmentDto
            {
                Id = a.Id, EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee != null ? a.Employee.LastName + " " + a.Employee.FirstName : null,
                AssignedFromUtc = a.AssignedFromUtc, AssignedToUtc = a.AssignedToUtc,
                IsPrimary = a.IsPrimary
            })
            .ToListAsync(ct);
}

public record GetAssignmentsByEmployeeQuery(Guid EmployeeId) : IRequest<IReadOnlyList<DriverAssignmentDto>>;

public class GetAssignmentsByEmployeeQueryHandler : IRequestHandler<GetAssignmentsByEmployeeQuery, IReadOnlyList<DriverAssignmentDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAssignmentsByEmployeeQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<DriverAssignmentDto>> Handle(GetAssignmentsByEmployeeQuery r, CancellationToken ct)
        => await _context.VehicleDriverAssignments.AsNoTracking()
            .Where(a => a.EmployeeId == r.EmployeeId).OrderByDescending(a => a.AssignedFromUtc)
            .Select(a => new DriverAssignmentDto
            {
                Id = a.Id, VehicleId = a.VehicleId,
                AssignedFromUtc = a.AssignedFromUtc, AssignedToUtc = a.AssignedToUtc,
                IsPrimary = a.IsPrimary
            })
            .ToListAsync(ct);
}
