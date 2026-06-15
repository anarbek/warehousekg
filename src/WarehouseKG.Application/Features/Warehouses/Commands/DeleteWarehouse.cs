using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Warehouses.Commands;

public record DeleteWarehouseCommand(Guid Id) : IRequest<bool>;

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteWarehouseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse is null)
        {
            return false;
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
