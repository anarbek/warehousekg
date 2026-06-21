using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.InventoryItems.Commands;

public record UpdateInventoryItemCommand(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string? Barcode,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    decimal ReorderLevel,
    decimal UnitPrice,
    bool IsActive) : IRequest<bool>;

public class UpdateInventoryItemCommandHandler : IRequestHandler<UpdateInventoryItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateInventoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item is null)
        {
            return false;
        }

        item.Sku = request.Sku;
        item.Name = request.Name;
        item.Description = request.Description;
        item.Barcode = request.Barcode;
        item.CategoryId = request.CategoryId;
        item.UnitOfMeasureId = request.UnitOfMeasureId;
        item.ReorderLevel = request.ReorderLevel;
        item.UnitPrice = request.UnitPrice;
        item.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
