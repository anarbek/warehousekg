using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Tenants.DTOs;

namespace WarehouseKG.Application.Tenants.Queries;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetTenantByIdQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tenant is null) return null;

        var userCount = await _identityService.GetUserCountForTenantAsync(tenant.Id, cancellationToken);
        var adminUserId = await _identityService.GetAdminUserIdForTenantAsync(tenant.Id, cancellationToken);
        var adminUserName = await _identityService.GetAdminUserNameForTenantAsync(tenant.Id, cancellationToken);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            DefaultCurrency = tenant.DefaultCurrency,
            Status = (int)tenant.Status,
            MaxUsers = tenant.MaxUsers,
            EnabledModules = tenant.EnabledModules,
            CreatedAt = tenant.CreatedAt,
            UserCount = userCount,
            AdminUserId = adminUserId,
            AdminUserName = adminUserName
        };
    }
}
