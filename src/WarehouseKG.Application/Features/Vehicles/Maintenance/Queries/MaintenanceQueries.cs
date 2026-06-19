using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Application.Features.Vehicles.Maintenance.Queries;

public record GetMaintenanceRecordsQuery(Guid VehicleId) : IRequest<IReadOnlyList<MaintenanceRecordDto>>;

public class GetMaintenanceRecordsQueryHandler : IRequestHandler<GetMaintenanceRecordsQuery, IReadOnlyList<MaintenanceRecordDto>>
{
    private readonly IApplicationDbContext _context;
    public GetMaintenanceRecordsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<MaintenanceRecordDto>> Handle(GetMaintenanceRecordsQuery r, CancellationToken ct)
        => await _context.VehicleMaintenanceRecords.AsNoTracking()
            .Where(m => m.VehicleId == r.VehicleId).OrderByDescending(m => m.Date)
            .Select(m => new MaintenanceRecordDto
            {
                Id = m.Id, MaintenanceType = m.MaintenanceType.ToString(),
                Date = m.Date, MileageKm = m.MileageKm, Cost = m.Cost,
                Description = m.Description, ServiceProvider = m.ServiceProvider,
                Notes = m.Notes, NextDueMileageKm = m.NextDueMileageKm,
                NextDueDate = m.NextDueDate
            })
            .ToListAsync(ct);
}
