using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.Insurance.Commands;

public record DeleteInsuranceRecordCommand(Guid Id) : IRequest<bool>;

public class DeleteInsuranceRecordCommandHandler : IRequestHandler<DeleteInsuranceRecordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public DeleteInsuranceRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(DeleteInsuranceRecordCommand r, CancellationToken ct)
    {
        var i = await _context.VehicleInsuranceRecords.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (i is null) return false;
        _context.VehicleInsuranceRecords.Remove(i);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
