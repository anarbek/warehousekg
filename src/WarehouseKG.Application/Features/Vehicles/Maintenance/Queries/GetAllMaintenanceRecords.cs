using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Application.Features.Vehicles.Maintenance.Queries;

public record GetAllMaintenanceRecordsQuery : IRequest<IReadOnlyList<MaintenanceRecordDto>>;

public class GetAllMaintenanceRecordsQueryHandler : IRequestHandler<GetAllMaintenanceRecordsQuery, IReadOnlyList<MaintenanceRecordDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllMaintenanceRecordsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<MaintenanceRecordDto>> Handle(GetAllMaintenanceRecordsQuery r, CancellationToken ct)
        => await _context.VehicleMaintenanceRecords.AsNoTracking()
            .OrderByDescending(m => m.Date)
            .Select(m => new MaintenanceRecordDto
            {
                Id = m.Id, MaintenanceType = m.MaintenanceType.ToString(),
                Date = m.Date, MileageKm = m.MileageKm, Cost = m.Cost,
                Description = m.Description, ServiceProvider = m.ServiceProvider,
                Notes = m.Notes, NextDueMileageKm = m.NextDueMileageKm,
                NextDueDate = m.NextDueDate,
                VehicleCode = m.Vehicle != null ? m.Vehicle.Code : null,
                VehiclePlate = m.Vehicle != null ? m.Vehicle.LicensePlate : null
            })
            .ToListAsync(ct);
}
