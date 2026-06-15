using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockReceipts.Commands;

public record CompleteStockReceiptCommand(Guid Id) : IRequest<OperationResult>;

public class CompleteStockReceiptCommandHandler : IRequestHandler<CompleteStockReceiptCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CompleteStockReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CompleteStockReceiptCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _context.StockReceipts
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (receipt is null)
        {
            return OperationResult.NotFound;
        }

        if (receipt.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        // Receiving increases stock on hand for each received item.
        var itemIds = receipt.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var items = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        foreach (var line in receipt.Lines)
        {
            var item = items.FirstOrDefault(i => i.Id == line.InventoryItemId);
            if (item is not null)
            {
                item.QuantityOnHand += line.Quantity;
            }
        }

        receipt.Status = StockOperationStatus.Completed;
        receipt.ReceivedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

public record CancelStockReceiptCommand(Guid Id) : IRequest<OperationResult>;

public class CancelStockReceiptCommandHandler : IRequestHandler<CancelStockReceiptCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelStockReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CancelStockReceiptCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _context.StockReceipts
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (receipt is null)
        {
            return OperationResult.NotFound;
        }

        if (receipt.Status != StockOperationStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        receipt.Status = StockOperationStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
