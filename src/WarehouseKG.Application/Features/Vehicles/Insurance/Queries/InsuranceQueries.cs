using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Application.Features.Vehicles.Insurance.Queries;

public record GetInsuranceRecordsQuery(Guid VehicleId) : IRequest<IReadOnlyList<InsuranceRecordDto>>;

public class GetInsuranceRecordsQueryHandler : IRequestHandler<GetInsuranceRecordsQuery, IReadOnlyList<InsuranceRecordDto>>
{
    private readonly IApplicationDbContext _context;
    public GetInsuranceRecordsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<InsuranceRecordDto>> Handle(GetInsuranceRecordsQuery r, CancellationToken ct)
        => await _context.VehicleInsuranceRecords.AsNoTracking()
            .Where(i => i.VehicleId == r.VehicleId).OrderByDescending(i => i.StartDate)
            .Select(i => new InsuranceRecordDto
            {
                Id = i.Id, PolicyNumber = i.PolicyNumber, Provider = i.Provider,
                CoverageType = i.CoverageType, StartDate = i.StartDate,
                EndDate = i.EndDate, PremiumAmount = i.PremiumAmount,
                Description = i.Description
            })
            .ToListAsync(ct);
}
