using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.StockReceipts.Commands;

public record DeleteStockReceiptCommand(Guid Id) : IRequest<OperationResult>;

public class DeleteStockReceiptCommandHandler : IRequestHandler<DeleteStockReceiptCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public DeleteStockReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(DeleteStockReceiptCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _context.StockReceipts
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (receipt is null)
            return OperationResult.NotFound;

        // Reversal: if completed, deduct the quantities that were added
        if (receipt.Status == Domain.Enums.StockOperationStatus.Completed)
        {
            var itemIds = receipt.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
            var items = await _context.InventoryItems
                .Where(i => itemIds.Contains(i.Id))
                .ToListAsync(cancellationToken);

            foreach (var line in receipt.Lines)
            {
                var item = items.FirstOrDefault(i => i.Id == line.InventoryItemId);
                if (item is not null)
                {
                    item.QuantityOnHand -= line.Quantity;
                }
            }
        }

        _context.StockReceipts.Remove(receipt);
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
