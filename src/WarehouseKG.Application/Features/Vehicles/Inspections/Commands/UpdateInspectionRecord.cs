using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Vehicles.Inspections.Commands;

public record UpdateInspectionRecordCommand(
    Guid Id, DateTime InspectionDate, DateTime ExpiryDate,
    InspectionResult Result, string? Inspector, string? Notes) : IRequest<bool>;

public class UpdateInspectionRecordCommandHandler : IRequestHandler<UpdateInspectionRecordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateInspectionRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateInspectionRecordCommand r, CancellationToken ct)
    {
        var i = await _context.VehicleInspectionRecords.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (i is null) return false;
        i.InspectionDate = DateTime.SpecifyKind(r.InspectionDate, DateTimeKind.Utc); i.ExpiryDate = DateTime.SpecifyKind(r.ExpiryDate, DateTimeKind.Utc);
        i.Result = r.Result; i.Inspector = r.Inspector; i.Notes = r.Notes;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
