using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Tenants.DTOs;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Tenants.Commands;

public record UpdateTenantCommand(
    Guid Id,
    string Name,
    string Slug,
    string? ContactEmail,
    string? ContactPhone,
    string DefaultCurrency,
    int? MaxUsers,
    string? EnabledModules
) : IRequest<bool>;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tenant is null) return false;

        tenant.Name = request.Name;
        tenant.Slug = request.Slug;
        tenant.ContactEmail = request.ContactEmail;
        tenant.ContactPhone = request.ContactPhone;
        tenant.DefaultCurrency = request.DefaultCurrency;
        tenant.MaxUsers = request.MaxUsers;
        tenant.EnabledModules = request.EnabledModules;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
