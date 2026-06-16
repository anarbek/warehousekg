using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.UnitsOfMeasure.Dtos;

namespace WarehouseKG.Application.Features.UnitsOfMeasure.Queries;

public record GetUnitsOfMeasureQuery : IRequest<IReadOnlyList<UnitOfMeasureDto>>;

public class GetUnitsOfMeasureQueryHandler
    : IRequestHandler<GetUnitsOfMeasureQuery, IReadOnlyList<UnitOfMeasureDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUnitsOfMeasureQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UnitOfMeasureDto>> Handle(
        GetUnitsOfMeasureQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.UnitsOfMeasure
            .AsNoTracking()
            .OrderBy(u => u.Code)
            .Select(u => new UnitOfMeasureDto
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                Description = u.Description
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetUnitOfMeasureByIdQuery(Guid Id) : IRequest<UnitOfMeasureDto?>;

public class GetUnitOfMeasureByIdQueryHandler
    : IRequestHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureDto?>
{
    private readonly IApplicationDbContext _context;

    public GetUnitOfMeasureByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UnitOfMeasureDto?> Handle(
        GetUnitOfMeasureByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.UnitsOfMeasure
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
            .Select(u => new UnitOfMeasureDto
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                Description = u.Description
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
