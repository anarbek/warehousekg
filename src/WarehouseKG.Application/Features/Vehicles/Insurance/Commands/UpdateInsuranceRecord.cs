using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Vehicles.Insurance.Commands;

public record UpdateInsuranceRecordCommand(
    Guid Id, string PolicyNumber, string Provider, string? CoverageType,
    DateTime StartDate, DateTime EndDate, decimal PremiumAmount,
    string? Description) : IRequest<bool>;

public class UpdateInsuranceRecordCommandHandler : IRequestHandler<UpdateInsuranceRecordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateInsuranceRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateInsuranceRecordCommand r, CancellationToken ct)
    {
        var i = await _context.VehicleInsuranceRecords.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (i is null) return false;
        i.PolicyNumber = r.PolicyNumber; i.Provider = r.Provider;
        i.CoverageType = r.CoverageType; i.StartDate = DateTime.SpecifyKind(r.StartDate, DateTimeKind.Utc);
        i.EndDate = DateTime.SpecifyKind(r.EndDate, DateTimeKind.Utc); i.PremiumAmount = r.PremiumAmount;
        i.Description = r.Description;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
