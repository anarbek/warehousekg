using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Application.Features.Vehicles.Inspections.Queries;

public record GetAllInspectionRecordsQuery : IRequest<IReadOnlyList<InspectionRecordDto>>;

public class GetAllInspectionRecordsQueryHandler : IRequestHandler<GetAllInspectionRecordsQuery, IReadOnlyList<InspectionRecordDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllInspectionRecordsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<InspectionRecordDto>> Handle(GetAllInspectionRecordsQuery r, CancellationToken ct)
        => await _context.VehicleInspectionRecords.AsNoTracking()
            .OrderByDescending(i => i.InspectionDate)
            .Select(i => new InspectionRecordDto
            {
                Id = i.Id, InspectionDate = i.InspectionDate, ExpiryDate = i.ExpiryDate,
                Result = i.Result.ToString(), Inspector = i.Inspector, Notes = i.Notes,
                VehicleCode = i.Vehicle != null ? i.Vehicle.Code : null,
                VehiclePlate = i.Vehicle != null ? i.Vehicle.LicensePlate : null
            })
            .ToListAsync(ct);
}
