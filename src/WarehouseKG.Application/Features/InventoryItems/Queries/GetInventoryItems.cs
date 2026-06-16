using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.InventoryItems.Dtos;

namespace WarehouseKG.Application.Features.InventoryItems.Queries;

public record GetInventoryItemsQuery : IRequest<IReadOnlyList<InventoryItemDto>>;

public class GetInventoryItemsQueryHandler
    : IRequestHandler<GetInventoryItemsQuery, IReadOnlyList<InventoryItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InventoryItemDto>> Handle(
        GetInventoryItemsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .OrderBy(i => i.Sku)
            .Select(i => new InventoryItemDto
            {
                Id = i.Id,
                Sku = i.Sku,
                Name = i.Name,
                Description = i.Description,
                Barcode = i.Barcode,
                CategoryId = i.CategoryId,
                CategoryName = i.Category != null ? i.Category.Name : null,
                UnitOfMeasureId = i.UnitOfMeasureId,
                UnitOfMeasureName = i.UnitOfMeasure != null ? i.UnitOfMeasure.Name : null,
                QuantityOnHand = i.QuantityOnHand,
                ReorderLevel = i.ReorderLevel,
                IsActive = i.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
