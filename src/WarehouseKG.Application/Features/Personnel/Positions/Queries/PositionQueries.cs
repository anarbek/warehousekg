using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Personnel.Positions.Dtos;

namespace WarehouseKG.Application.Features.Personnel.Positions.Queries;

public record GetPositionsQuery : IRequest<IReadOnlyList<PositionDto>>;

public class GetPositionsQueryHandler : IRequestHandler<GetPositionsQuery, IReadOnlyList<PositionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPositionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PositionDto>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Positions
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new PositionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetPositionByIdQuery(Guid Id) : IRequest<PositionDto?>;

public class GetPositionByIdQueryHandler : IRequestHandler<GetPositionByIdQuery, PositionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPositionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PositionDto?> Handle(GetPositionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Positions
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PositionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
