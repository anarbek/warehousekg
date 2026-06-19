using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Personnel.Shifts.Dtos;

namespace WarehouseKG.Application.Features.Personnel.Shifts.Queries;

public record GetShiftsQuery : IRequest<IReadOnlyList<ShiftDto>>;

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, IReadOnlyList<ShiftDto>>
{
    private readonly IApplicationDbContext _context;
    public GetShiftsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<ShiftDto>> Handle(GetShiftsQuery r, CancellationToken ct)
        => await _context.Shifts.AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new ShiftDto { Id = x.Id, Code = x.Code, Name = x.Name, StartTime = x.StartTime, EndTime = x.EndTime, IsActive = x.IsActive })
            .ToListAsync(ct);
}

public record GetShiftByIdQuery(Guid Id) : IRequest<ShiftDto?>;

public class GetShiftByIdQueryHandler : IRequestHandler<GetShiftByIdQuery, ShiftDto?>
{
    private readonly IApplicationDbContext _context;
    public GetShiftByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<ShiftDto?> Handle(GetShiftByIdQuery r, CancellationToken ct)
        => await _context.Shifts.AsNoTracking().Where(x => x.Id == r.Id)
            .Select(x => new ShiftDto { Id = x.Id, Code = x.Code, Name = x.Name, StartTime = x.StartTime, EndTime = x.EndTime, IsActive = x.IsActive })
            .FirstOrDefaultAsync(ct);
}
