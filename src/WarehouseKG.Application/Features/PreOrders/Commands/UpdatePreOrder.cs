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
    IReadOnlyList<PreOrderLineInput> Lines,
    decimal? AmountPlanned = null,
    decimal? AmountPaid = null) : IRequest<OperationResult>;

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
        preOrder.AmountPlanned = request.AmountPlanned ?? preOrder.AmountPlanned;
        preOrder.AmountPaid = request.AmountPaid ?? preOrder.AmountPaid;

        // Capture warehouse stock for line items
        var itemIds = request.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var stockQuantities = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.QuantityOnHand, cancellationToken);

        // Delete old lines directly (avoid EF Core navigation/tracking conflicts)
        var oldLines = await _context.PreOrderLines
            .Where(l => l.PreOrderId == request.Id)
            .ToListAsync(cancellationToken);
        _context.PreOrderLines.RemoveRange(oldLines);

        // Add new lines from request
        foreach (var input in request.Lines)
        {
            var stock = stockQuantities.GetValueOrDefault(input.InventoryItemId, 0m);
            var lineTotal = input.Quantity * input.UnitPrice * (1 - input.DiscountPercent / 100);
            _context.PreOrderLines.Add(new PreOrderLine
            {
                Id = Guid.NewGuid(),
                PreOrderId = request.Id,
                InventoryItemId = input.InventoryItemId,
                Quantity = input.Quantity,
                UnitPrice = input.UnitPrice,
                WarehouseStockSnapshot = stock,
                StockDifference = stock - input.Quantity,
                DiscountPercent = input.DiscountPercent,
                LineTotal = lineTotal,
            });
        }

        preOrder.TotalAmount = request.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100));

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
