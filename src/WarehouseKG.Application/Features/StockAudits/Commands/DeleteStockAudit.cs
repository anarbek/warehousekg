using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.StockAudits.Commands;

public record DeleteStockAuditCommand(Guid Id) : IRequest<bool>;

public class DeleteStockAuditCommandHandler : IRequestHandler<DeleteStockAuditCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteStockAuditCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteStockAuditCommand request, CancellationToken cancellationToken)
    {
        var audit = await _context.StockAudits
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (audit is null) return false;

        _context.StockAuditLines.RemoveRange(audit.Lines);
        _context.StockAudits.Remove(audit);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
