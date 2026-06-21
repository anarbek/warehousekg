using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.PreOrders.Dtos;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PreOrders.Commands;

public record CreatePreOrderCommand(
    string Number,
    Guid CustomerId,
    Guid WarehouseId,
    string PaymentType,
    string? Currency,
    DateTime? ExpectedDateUtc,
    string? Notes,
    IReadOnlyList<PreOrderLineInput> Lines,
    decimal? AmountPlanned = null,
    decimal? AmountPaid = null) : IRequest<Guid>;

public class CreatePreOrderCommandHandler : IRequestHandler<CreatePreOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreatePreOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreatePreOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
        {
            throw new InvalidOperationException("Pre-order must have at least one line.");
        }

        // Capture warehouse stock for each item
        var itemIds = request.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var stockQuantities = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.QuantityOnHand, cancellationToken);

        var preOrder = new PreOrder
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            CustomerId = request.CustomerId,
            PresellerId = _currentUser.EmployeeId,
            WarehouseId = request.WarehouseId,
            PaymentType = request.PaymentType,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "KGS" : request.Currency,
            OrderDateUtc = DateTime.UtcNow,
            ExpectedDateUtc = request.ExpectedDateUtc.HasValue ? DateTime.SpecifyKind(request.ExpectedDateUtc.Value, DateTimeKind.Utc) : null,
            Notes = request.Notes,
            Status = PreOrderStatus.Draft,
            AmountPlanned = request.AmountPlanned ?? 0,
            AmountPaid = request.AmountPaid ?? 0,
            Lines = request.Lines.Select(l =>
            {
                var stock = stockQuantities.GetValueOrDefault(l.InventoryItemId, 0m);
                var lineTotal = l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100);
                return new PreOrderLine
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = l.InventoryItemId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    WarehouseStockSnapshot = stock,
                    StockDifference = stock - l.Quantity,
                    DiscountPercent = l.DiscountPercent,
                    LineTotal = lineTotal,
                };
            }).ToList()
        };

        preOrder.TotalAmount = preOrder.Lines.Sum(l => l.LineTotal);

        _context.PreOrders.Add(preOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return preOrder.Id;
    }
}
