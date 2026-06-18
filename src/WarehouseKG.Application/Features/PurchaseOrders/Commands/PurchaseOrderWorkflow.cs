using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PurchaseOrders.Commands;

public record SubmitPurchaseOrderCommand(Guid Id) : IRequest<OperationResult>;

public class SubmitPurchaseOrderCommandHandler : IRequestHandler<SubmitPurchaseOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public SubmitPurchaseOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return OperationResult.NotFound;
        }

        if (order.Status != PurchaseOrderStatus.Draft)
        {
            return OperationResult.InvalidState;
        }

        order.Status = PurchaseOrderStatus.Submitted;
        order.SubmittedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

// Receiving a submitted PO records the goods as arrived and increases stock on hand.
public record ReceivePurchaseOrderCommand(Guid Id) : IRequest<OperationResult>;

public class ReceivePurchaseOrderCommandHandler : IRequestHandler<ReceivePurchaseOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public ReceivePurchaseOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return OperationResult.NotFound;
        }

        if (order.Status != PurchaseOrderStatus.Submitted)
        {
            return OperationResult.InvalidState;
        }

        var itemIds = order.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var items = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        foreach (var line in order.Lines)
        {
            var item = items.FirstOrDefault(i => i.Id == line.InventoryItemId);
            if (item is not null)
            {
                item.QuantityOnHand += line.Quantity;
            }
        }

        order.Status = PurchaseOrderStatus.Received;
        order.ReceivedAtUtc ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}

public record CancelPurchaseOrderCommand(Guid Id) : IRequest<OperationResult>;

public class CancelPurchaseOrderCommandHandler : IRequestHandler<CancelPurchaseOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelPurchaseOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return OperationResult.NotFound;
        }

        // A received order cannot be cancelled; only Draft or Submitted orders may be.
        if (order.Status is not (PurchaseOrderStatus.Draft or PurchaseOrderStatus.Submitted))
        {
            return OperationResult.InvalidState;
        }

        order.Status = PurchaseOrderStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success;
    }
}
