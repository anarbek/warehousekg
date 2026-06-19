using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Application.Features.Vehicles.Inspections.Queries;

public record GetInspectionRecordsQuery(Guid VehicleId) : IRequest<IReadOnlyList<InspectionRecordDto>>;

public class GetInspectionRecordsQueryHandler : IRequestHandler<GetInspectionRecordsQuery, IReadOnlyList<InspectionRecordDto>>
{
    private readonly IApplicationDbContext _context;
    public GetInspectionRecordsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<InspectionRecordDto>> Handle(GetInspectionRecordsQuery r, CancellationToken ct)
        => await _context.VehicleInspectionRecords.AsNoTracking()
            .Where(i => i.VehicleId == r.VehicleId).OrderByDescending(i => i.InspectionDate)
            .Select(i => new InspectionRecordDto
            {
                Id = i.Id, InspectionDate = i.InspectionDate, ExpiryDate = i.ExpiryDate,
                Result = i.Result.ToString(), Inspector = i.Inspector, Notes = i.Notes
            })
            .ToListAsync(ct);
}
