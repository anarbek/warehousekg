using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Tenants.Commands;

public record ActivateTenantCommand(Guid Id) : IRequest<bool>;

public class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ActivateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tenant is null) return false;

        tenant.Status = TenantStatus.Active;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
