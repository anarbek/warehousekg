using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Warehouses.Dtos;

namespace WarehouseKG.Application.Features.Warehouses.Queries;

public record GetWarehousesQuery : IRequest<IReadOnlyList<WarehouseDto>>;

public class GetWarehousesQueryHandler
    : IRequestHandler<GetWarehousesQuery, IReadOnlyList<WarehouseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWarehousesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WarehouseDto>> Handle(
        GetWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Code)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                IsActive = w.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
