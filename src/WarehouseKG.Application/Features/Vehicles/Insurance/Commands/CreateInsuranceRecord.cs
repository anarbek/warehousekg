using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.Vehicles.Insurance.Commands;

public record CreateInsuranceRecordCommand(
    Guid VehicleId, string PolicyNumber, string Provider, string? CoverageType,
    DateTime StartDate, DateTime EndDate, decimal PremiumAmount,
    string? Description) : IRequest<Guid>;

public class CreateInsuranceRecordCommandHandler : IRequestHandler<CreateInsuranceRecordCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateInsuranceRecordCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateInsuranceRecordCommand r, CancellationToken ct)
    {
        var i = new VehicleInsuranceRecord
        {
            Id = Guid.NewGuid(), VehicleId = r.VehicleId,
            PolicyNumber = r.PolicyNumber, Provider = r.Provider,
            CoverageType = r.CoverageType, StartDate = DateTime.SpecifyKind(r.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(r.EndDate, DateTimeKind.Utc), PremiumAmount = r.PremiumAmount,
            Description = r.Description
        };
        _context.VehicleInsuranceRecords.Add(i);

        var v = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == r.VehicleId, ct);
        if (v is not null) v.InsuranceExpiryDate = DateTime.SpecifyKind(r.EndDate, DateTimeKind.Utc);

        await _context.SaveChangesAsync(ct);
        return i.Id;
    }
}
