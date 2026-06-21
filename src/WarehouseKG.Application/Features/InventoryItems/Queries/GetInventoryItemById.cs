using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.InventoryItems.Dtos;

namespace WarehouseKG.Application.Features.InventoryItems.Queries;

public record GetInventoryItemByIdQuery(Guid Id) : IRequest<InventoryItemDto?>;

public class GetInventoryItemByIdQueryHandler
    : IRequestHandler<GetInventoryItemByIdQuery, InventoryItemDto?>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryItemByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItemDto?> Handle(
        GetInventoryItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.Id == request.Id)
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
                UnitPrice = i.UnitPrice,
                IsActive = i.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
