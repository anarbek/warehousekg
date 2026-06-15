using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockReceipts.Commands;

public record StockReceiptLineInput(
    Guid InventoryItemId,
    Guid? WarehouseLocationId,
    decimal Quantity);

public record CreateStockReceiptCommand(
    string Number,
    Guid WarehouseId,
    string? SupplierReference,
    string? Notes,
    IReadOnlyList<StockReceiptLineInput> Lines) : IRequest<Guid>;

public class CreateStockReceiptCommandHandler : IRequestHandler<CreateStockReceiptCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStockReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStockReceiptCommand request, CancellationToken cancellationToken)
    {
        var receipt = new StockReceipt
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            WarehouseId = request.WarehouseId,
            SupplierReference = request.SupplierReference,
            Notes = request.Notes,
            Status = StockOperationStatus.Draft,
            Lines = request.Lines.Select(l => new StockReceiptLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                WarehouseLocationId = l.WarehouseLocationId,
                Quantity = l.Quantity
            }).ToList()
        };

        _context.StockReceipts.Add(receipt);
        await _context.SaveChangesAsync(cancellationToken);

        return receipt.Id;
    }
}
