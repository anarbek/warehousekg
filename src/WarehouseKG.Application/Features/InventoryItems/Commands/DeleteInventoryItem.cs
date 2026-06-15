using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.InventoryItems.Commands;

public record DeleteInventoryItemCommand(Guid Id) : IRequest<bool>;

public class DeleteInventoryItemCommandHandler : IRequestHandler<DeleteInventoryItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteInventoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item is null)
        {
            return false;
        }

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
