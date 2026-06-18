using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PurchaseOrders.Commands;

public record PurchaseOrderLineInput(
    Guid InventoryItemId,
    decimal Quantity,
    decimal UnitPrice);

public record CreatePurchaseOrderCommand(
    string Number,
    Guid SupplierId,
    Guid? WarehouseId,
    string? Currency,
    DateTime? ReceivedAtUtc,
    string? Notes,
    IReadOnlyList<PurchaseOrderLineInput> Lines) : IRequest<Guid>;

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePurchaseOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "KGS" : request.Currency,
            OrderDateUtc = DateTime.UtcNow,
            ReceivedAtUtc = request.ReceivedAtUtc,
            Notes = request.Notes,
            Status = PurchaseOrderStatus.Draft,
            Lines = request.Lines.Select(l => new PurchaseOrderLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return purchaseOrder.Id;
    }
}
