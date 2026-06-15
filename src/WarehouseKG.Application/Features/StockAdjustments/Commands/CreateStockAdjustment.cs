using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockAdjustments.Commands;

public record StockAdjustmentLineInput(
    Guid InventoryItemId,
    decimal QuantityChange,
    string? Notes);

public record CreateStockAdjustmentCommand(
    string Number,
    Guid WarehouseId,
    StockAdjustmentReason Reason,
    string? Notes,
    IReadOnlyList<StockAdjustmentLineInput> Lines) : IRequest<Guid>;

public class CreateStockAdjustmentCommandHandler : IRequestHandler<CreateStockAdjustmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStockAdjustmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adjustment = new StockAdjustment
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            WarehouseId = request.WarehouseId,
            Reason = request.Reason,
            Notes = request.Notes,
            Status = StockOperationStatus.Draft,
            Lines = request.Lines.Select(l => new StockAdjustmentLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                QuantityChange = l.QuantityChange,
                Notes = l.Notes
            }).ToList()
        };

        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync(cancellationToken);

        return adjustment.Id;
    }
}
