using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.PackOrders.Dtos;

namespace WarehouseKG.Application.Features.PackOrders.Queries;

public record GetPackOrdersQuery : IRequest<IReadOnlyList<PackOrderSummaryDto>>;

public class GetPackOrdersQueryHandler
    : IRequestHandler<GetPackOrdersQuery, IReadOnlyList<PackOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPackOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PackOrderSummaryDto>> Handle(
        GetPackOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PackOrders
            .AsNoTracking()
            .OrderByDescending(p => p.PackedAtUtc)
            .ThenBy(p => p.Number)
            .Select(p => new PackOrderSummaryDto
            {
                Id = p.Id,
                Number = p.Number,
                WarehouseId = p.WarehouseId,
                WarehouseName = p.Warehouse != null ? p.Warehouse.Name : null,
                Status = p.Status,
                PackedAtUtc = p.PackedAtUtc,
                ActualPackDate = p.ActualPackDate,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CreatedBy = p.CreatedBy,
                UpdatedBy = p.UpdatedBy,
                LineCount = p.Lines.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetPackOrderByIdQuery(Guid Id) : IRequest<PackOrderDto?>;

public class GetPackOrderByIdQueryHandler
    : IRequestHandler<GetPackOrderByIdQuery, PackOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPackOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PackOrderDto?> Handle(
        GetPackOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PackOrders
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PackOrderDto
            {
                Id = p.Id,
                Number = p.Number,
                WarehouseId = p.WarehouseId,
                WarehouseName = p.Warehouse != null ? p.Warehouse.Name : null,
                PickOrderId = p.PickOrderId,
                Status = p.Status,
                PackedAtUtc = p.PackedAtUtc,
                ActualPackDate = p.ActualPackDate,
                Notes = p.Notes,
                Lines = p.Lines.Select(l => new PackOrderLineDto
                {
                    Id = l.Id,
                    InventoryItemId = l.InventoryItemId,
                    Quantity = l.Quantity,
                    PackageLabel = l.PackageLabel
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
