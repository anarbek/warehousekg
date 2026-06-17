using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.InventoryItems.Commands;

/// <summary>
/// Creates an inventory item. If warehouseId is provided, an initial StockReceipt
/// is auto-created and completed so stock enters through receiving.
/// </summary>
public record CreateInventoryItemCommand(
    string Sku,
    string Name,
    string? Description,
    string? Barcode,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    decimal ReorderLevel,
    bool IsActive = true,
    Guid? WarehouseId = null,
    decimal InitialQuantity = 0) : IRequest<Guid>;

public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateInventoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            Barcode = request.Barcode,
            CategoryId = request.CategoryId,
            UnitOfMeasureId = request.UnitOfMeasureId,
            QuantityOnHand = 0,
            ReorderLevel = request.ReorderLevel,
            IsActive = request.IsActive
        };

        _context.InventoryItems.Add(item);

        // If warehouse + initial quantity provided, create an auto-receipt
        if (request.WarehouseId.HasValue && request.InitialQuantity > 0)
        {
            var receipt = new StockReceipt
            {
                Id = Guid.NewGuid(),
                Number = $"RCV-{item.Sku}",
                WarehouseId = request.WarehouseId.Value,
                Status = StockOperationStatus.Completed,
                ReceivedAtUtc = DateTime.UtcNow,
                Notes = "Авто: начальный остаток",
                Lines = new List<StockReceiptLine>
                {
                    new StockReceiptLine
                    {
                        Id = Guid.NewGuid(),
                        InventoryItemId = item.Id,
                        Quantity = request.InitialQuantity
                    }
                }
            };

            item.QuantityOnHand = request.InitialQuantity;
            _context.StockReceipts.Add(receipt);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return item.Id;
    }
}
