using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Reports.Dtos;

namespace WarehouseKG.Application.Features.Reports.Queries;

public record GetInventorySummaryReportQuery : IRequest<InventorySummaryReportDto>;

public class GetInventorySummaryReportQueryHandler
    : IRequestHandler<GetInventorySummaryReportQuery, InventorySummaryReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetInventorySummaryReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventorySummaryReportDto> Handle(
        GetInventorySummaryReportQuery request,
        CancellationToken cancellationToken)
    {
        var items = _context.InventoryItems.AsNoTracking();

        return new InventorySummaryReportDto
        {
            TotalItems = await items.CountAsync(cancellationToken),
            ActiveItems = await items.CountAsync(i => i.IsActive, cancellationToken),
            TotalQuantityOnHand = await items.SumAsync(i => i.QuantityOnHand, cancellationToken),
            ItemsBelowReorder = await items.CountAsync(
                i => i.IsActive && i.QuantityOnHand <= i.ReorderLevel, cancellationToken),
            ItemsOutOfStock = await items.CountAsync(
                i => i.IsActive && i.QuantityOnHand <= 0, cancellationToken)
        };
    }
}

public record GetLowStockReportQuery : IRequest<IReadOnlyList<LowStockItemDto>>;

public class GetLowStockReportQueryHandler
    : IRequestHandler<GetLowStockReportQuery, IReadOnlyList<LowStockItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLowStockReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LowStockItemDto>> Handle(
        GetLowStockReportQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.IsActive && i.QuantityOnHand <= i.ReorderLevel)
            .OrderByDescending(i => i.ReorderLevel - i.QuantityOnHand)
            .ThenBy(i => i.Sku)
            .Select(i => new LowStockItemDto
            {
                Id = i.Id,
                Sku = i.Sku,
                Name = i.Name,
                CategoryId = i.CategoryId,
                QuantityOnHand = i.QuantityOnHand,
                ReorderLevel = i.ReorderLevel,
                Deficit = i.ReorderLevel - i.QuantityOnHand
            })
            .ToListAsync(cancellationToken);
    }
}
