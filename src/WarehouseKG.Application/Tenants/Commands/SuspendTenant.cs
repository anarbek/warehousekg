using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Tenants.Commands;

public record SuspendTenantCommand(Guid Id) : IRequest<bool>;

public class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public SuspendTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tenant is null) return false;

        tenant.Status = TenantStatus.Suspended;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
