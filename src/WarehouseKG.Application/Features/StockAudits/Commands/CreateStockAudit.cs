using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockAudits.Commands;

public record StockAuditLineInput(
    Guid InventoryItemId,
    decimal CountedQuantity);

public record CreateStockAuditCommand(
    string Number,
    Guid WarehouseId,
    string? Notes,
    IReadOnlyList<StockAuditLineInput> Lines) : IRequest<Guid>;

public class CreateStockAuditCommandHandler : IRequestHandler<CreateStockAuditCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStockAuditCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStockAuditCommand request, CancellationToken cancellationToken)
    {
        // Snapshot the current on-hand figure for each counted item so the variance recorded on
        // the audit reflects the book quantity at the time the count was taken.
        var itemIds = request.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var onHandByItem = await _context.InventoryItems
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.QuantityOnHand, cancellationToken);

        var audit = new StockAudit
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            WarehouseId = request.WarehouseId,
            Notes = request.Notes,
            Status = StockOperationStatus.Draft,
            Lines = request.Lines.Select(l => new StockAuditLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                SystemQuantity = onHandByItem.TryGetValue(l.InventoryItemId, out var onHand) ? onHand : 0m,
                CountedQuantity = l.CountedQuantity
            }).ToList()
        };

        _context.StockAudits.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        return audit.Id;
    }
}
