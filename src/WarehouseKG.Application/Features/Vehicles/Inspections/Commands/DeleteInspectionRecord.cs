using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.Inspections.Commands;

public record DeleteInspectionRecordCommand(Guid Id) : IRequest<bool>;

public class DeleteInspectionRecordCommandHandler : IRequestHandler<DeleteInspectionRecordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteInspectionRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteInspectionRecordCommand r, CancellationToken ct)
    {
        var i = await _context.VehicleInspectionRecords.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (i is null) return false;
        _context.VehicleInspectionRecords.Remove(i);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
