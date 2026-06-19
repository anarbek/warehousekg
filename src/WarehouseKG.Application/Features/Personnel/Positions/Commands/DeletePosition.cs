using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Personnel.Positions.Commands;

public record DeletePositionCommand(Guid Id) : IRequest<bool>;

public class DeletePositionCommandHandler : IRequestHandler<DeletePositionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeletePositionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.Positions
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (position is null) return false;

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
