using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.PreOrders.Dtos;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PreOrders.Commands;

public record UpdatePreOrderCommand(
    Guid Id,
    string Number,
    Guid CustomerId,
    Guid WarehouseId,
    string PaymentType,
    string? Currency,
    DateTime? ExpectedDateUtc,
    string? Notes,
    IReadOnlyList<PreOrderLineInput> Lines) : IRequest<OperationResult>;

public class UpdatePreOrderCommandHandler : IRequestHandler<UpdatePreOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public UpdatePreOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> Handle(UpdatePreOrderCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.PreOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (preOrder is null) return OperationResult.NotFound;
        if (preOrder.Status != PreOrderStatus.Draft) return OperationResult.InvalidState;

        preOrder.Number = request.Number;
        preOrder.CustomerId = request.CustomerId;
        preOrder.WarehouseId = request.WarehouseId;
        preOrder.PaymentType = request.PaymentType;
        preOrder.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "KGS" : request.Currency;
        preOrder.ExpectedDateUtc = request.ExpectedDateUtc.HasValue ? DateTime.SpecifyKind(request.ExpectedDateUtc.Value, DateTimeKind.Utc) : null;
        preOrder.Notes = request.Notes;

        // Capture warehouse stock for line items
        var itemIds = request.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var stockQuantities = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.QuantityOnHand, cancellationToken);

        // Remove lines not in the request
        var requestItemIds = request.Lines.Select(l => l.InventoryItemId).ToHashSet();
        var toRemove = preOrder.Lines.Where(l => !requestItemIds.Contains(l.InventoryItemId)).ToList();
        foreach (var line in toRemove)
        {
            _context.PreOrderLines.Remove(line);
        }

        // Update existing or add new
        foreach (var input in request.Lines)
        {
            var existing = preOrder.Lines.FirstOrDefault(l => l.InventoryItemId == input.InventoryItemId);
            var stock = stockQuantities.GetValueOrDefault(input.InventoryItemId, 0m);
            var lineTotal = input.Quantity * input.UnitPrice * (1 - input.DiscountPercent / 100);

            if (existing is not null)
            {
                existing.Quantity = input.Quantity;
                existing.UnitPrice = input.UnitPrice;
                existing.WarehouseStockSnapshot = stock;
                existing.StockDifference = stock - input.Quantity;
                existing.DiscountPercent = input.DiscountPercent;
                existing.LineTotal = lineTotal;
            }
            else
            {
                preOrder.Lines.Add(new PreOrderLine
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = input.InventoryItemId,
                    Quantity = input.Quantity,
                    UnitPrice = input.UnitPrice,
                    WarehouseStockSnapshot = stock,
                    StockDifference = stock - input.Quantity,
                    DiscountPercent = input.DiscountPercent,
                    LineTotal = lineTotal,
                });
            }
        }

        preOrder.TotalAmount = preOrder.Lines.Sum(l => l.LineTotal);

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}

public record DeletePreOrderCommand(Guid Id) : IRequest<OperationResult>;

public class DeletePreOrderCommandHandler : IRequestHandler<DeletePreOrderCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public DeletePreOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(DeletePreOrderCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.PreOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (preOrder is null) return OperationResult.NotFound;
        if (preOrder.Status != PreOrderStatus.Draft) return OperationResult.InvalidState;

        _context.PreOrders.Remove(preOrder);
        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}
