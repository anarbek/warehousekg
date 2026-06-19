using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Vehicles.Maintenance.Commands;

public record UpdateMaintenanceRecordCommand(
    Guid Id, MaintenanceType MaintenanceType, DateTime Date, decimal MileageKm,
    decimal Cost, string? Description, string? ServiceProvider, string? Notes,
    decimal? NextDueMileageKm, DateTime? NextDueDate) : IRequest<bool>;

public class UpdateMaintenanceRecordCommandHandler : IRequestHandler<UpdateMaintenanceRecordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateMaintenanceRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateMaintenanceRecordCommand r, CancellationToken ct)
    {
        var m = await _context.VehicleMaintenanceRecords.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (m is null) return false;
        m.MaintenanceType = r.MaintenanceType; m.Date = DateTime.SpecifyKind(r.Date, DateTimeKind.Utc);
        m.MileageKm = r.MileageKm; m.Cost = r.Cost;
        m.Description = r.Description; m.ServiceProvider = r.ServiceProvider;
        m.Notes = r.Notes; m.NextDueMileageKm = r.NextDueMileageKm;
        m.NextDueDate = r.NextDueDate.HasValue ? DateTime.SpecifyKind(r.NextDueDate.Value, DateTimeKind.Utc) : null;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
