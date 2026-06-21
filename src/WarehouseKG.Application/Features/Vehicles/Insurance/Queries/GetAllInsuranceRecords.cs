using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Application.Features.Vehicles.Insurance.Queries;

public record GetAllInsuranceRecordsQuery : IRequest<IReadOnlyList<InsuranceRecordDto>>;

public class GetAllInsuranceRecordsQueryHandler : IRequestHandler<GetAllInsuranceRecordsQuery, IReadOnlyList<InsuranceRecordDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllInsuranceRecordsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<InsuranceRecordDto>> Handle(GetAllInsuranceRecordsQuery r, CancellationToken ct)
        => await _context.VehicleInsuranceRecords.AsNoTracking()
            .OrderByDescending(i => i.StartDate)
            .Select(i => new InsuranceRecordDto
            {
                Id = i.Id,
                VehicleId = i.VehicleId,
                PolicyNumber = i.PolicyNumber, Provider = i.Provider,
                CoverageType = i.CoverageType, StartDate = i.StartDate,
                EndDate = i.EndDate, PremiumAmount = i.PremiumAmount,
                Description = i.Description,
                VehicleCode = i.Vehicle != null ? i.Vehicle.Code : null,
                VehiclePlate = i.Vehicle != null ? i.Vehicle.LicensePlate : null
            })
            .ToListAsync(ct);
}
