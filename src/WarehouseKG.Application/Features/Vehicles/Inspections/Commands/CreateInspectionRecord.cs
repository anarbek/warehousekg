using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Vehicles.Inspections.Commands;

public record CreateInspectionRecordCommand(
    Guid VehicleId, DateTime InspectionDate, DateTime ExpiryDate,
    InspectionResult Result, string? Inspector, string? Notes) : IRequest<Guid>;

public class CreateInspectionRecordCommandHandler : IRequestHandler<CreateInspectionRecordCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateInspectionRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateInspectionRecordCommand r, CancellationToken ct)
    {
        var i = new VehicleInspectionRecord
        {
            Id = Guid.NewGuid(), VehicleId = r.VehicleId,
            InspectionDate = DateTime.SpecifyKind(r.InspectionDate, DateTimeKind.Utc), ExpiryDate = DateTime.SpecifyKind(r.ExpiryDate, DateTimeKind.Utc),
            Result = r.Result, Inspector = r.Inspector, Notes = r.Notes
        };
        _context.VehicleInspectionRecords.Add(i);

        var v = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == r.VehicleId, ct);
        if (v is not null) v.TechInspectionExpiryDate = DateTime.SpecifyKind(r.ExpiryDate, DateTimeKind.Utc);

        await _context.SaveChangesAsync(ct);
        return i.Id;
    }
}
