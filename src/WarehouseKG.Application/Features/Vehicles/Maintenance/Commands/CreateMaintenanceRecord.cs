using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Vehicles.Maintenance.Commands;

public record CreateMaintenanceRecordCommand(
    Guid VehicleId, MaintenanceType MaintenanceType, DateTime Date, decimal MileageKm,
    decimal Cost, string? Description, string? ServiceProvider, string? Notes,
    decimal? NextDueMileageKm, DateTime? NextDueDate) : IRequest<Guid>;

public class CreateMaintenanceRecordCommandHandler : IRequestHandler<CreateMaintenanceRecordCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateMaintenanceRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateMaintenanceRecordCommand r, CancellationToken ct)
    {
        var m = new VehicleMaintenanceRecord
        {
            Id = Guid.NewGuid(), VehicleId = r.VehicleId,
            MaintenanceType = r.MaintenanceType, Date = DateTime.SpecifyKind(r.Date, DateTimeKind.Utc),
            MileageKm = r.MileageKm, Cost = r.Cost,
            Description = r.Description, ServiceProvider = r.ServiceProvider,
            Notes = r.Notes, NextDueMileageKm = r.NextDueMileageKm,
            NextDueDate = r.NextDueDate.HasValue ? DateTime.SpecifyKind(r.NextDueDate.Value, DateTimeKind.Utc) : null
        };
        _context.VehicleMaintenanceRecords.Add(m);

        // Auto-update vehicle's next-maintenance fields
        var v = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == r.VehicleId, ct);
        if (v is not null)
        {
            if (r.NextDueMileageKm.HasValue) v.NextMaintenanceMileageKm = r.NextDueMileageKm;
            if (r.NextDueDate.HasValue) v.NextMaintenanceDate = DateTime.SpecifyKind(r.NextDueDate.Value, DateTimeKind.Utc);
        }

        await _context.SaveChangesAsync(ct);
        return m.Id;
    }
}
