using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Warehouses.Dtos;

namespace WarehouseKG.Application.Features.Warehouses.Queries;

public record GetWarehouseByIdQuery(Guid Id) : IRequest<WarehouseDto?>;

public class GetWarehouseByIdQueryHandler
    : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto?>
{
    private readonly IApplicationDbContext _context;

    public GetWarehouseByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WarehouseDto?> Handle(
        GetWarehouseByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.Id == request.Id)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                IsActive = w.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
