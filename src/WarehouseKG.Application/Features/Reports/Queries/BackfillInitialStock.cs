using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Reports.Queries;

/// <summary>
/// Creates initial StockReceipts for existing items that have QuantityOnHand > 0
/// but no prior receiving operations. This backfills the warehouse assignment gap.
/// </summary>
public record BackfillInitialStockCommand(Guid WarehouseId) : IRequest<int>;

public class BackfillInitialStockCommandHandler : IRequestHandler<BackfillInitialStockCommand, int>
{
    private readonly IApplicationDbContext _context;

    public BackfillInitialStockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(BackfillInitialStockCommand request, CancellationToken cancellationToken)
    {
        // Find items with QOH > 0 that have no completed StockReceipt at this warehouse
        var itemsWithStock = await _context.InventoryItems
            .Where(i => i.QuantityOnHand > 0)
            .ToListAsync(cancellationToken);

        var existingReceiptItemIds = await _context.StockReceiptLines
            .Include(l => l.StockReceipt)
            .Where(l => l.StockReceipt!.WarehouseId == request.WarehouseId
                && l.StockReceipt.Status == StockOperationStatus.Completed)
            .Select(l => l.InventoryItemId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var toBackfill = itemsWithStock
            .Where(i => !existingReceiptItemIds.Contains(i.Id))
            .ToList();

        foreach (var item in toBackfill)
        {
            var receipt = new StockReceipt
            {
                Id = Guid.NewGuid(),
                Number = $"RCV-BF-{item.Sku}",
                WarehouseId = request.WarehouseId,
                Status = StockOperationStatus.Completed,
                ReceivedAtUtc = DateTime.UtcNow,
                Notes = $"Авто-заполнение: начальный остаток {item.QuantityOnHand}",
                Lines = new List<StockReceiptLine>
                {
                    new StockReceiptLine
                    {
                        Id = Guid.NewGuid(),
                        InventoryItemId = item.Id,
                        Quantity = item.QuantityOnHand
                    }
                }
            };

            _context.StockReceipts.Add(receipt);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return toBackfill.Count;
    }
}
