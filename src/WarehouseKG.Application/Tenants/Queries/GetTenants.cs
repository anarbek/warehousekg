using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Tenants.DTOs;

namespace WarehouseKG.Application.Tenants.Queries;

public record GetTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetTenantsQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        // Get user counts per tenant via the identity service
        var tenantDtos = new List<TenantDto>();
        foreach (var t in tenants)
        {
            var userCount = await _identityService.GetUserCountForTenantAsync(t.Id, cancellationToken);
            var adminUserId = await _identityService.GetAdminUserIdForTenantAsync(t.Id, cancellationToken);
            var adminUserName = await _identityService.GetAdminUserNameForTenantAsync(t.Id, cancellationToken);
            tenantDtos.Add(new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                ContactEmail = t.ContactEmail,
                ContactPhone = t.ContactPhone,
                DefaultCurrency = t.DefaultCurrency,
                Status = (int)t.Status,
                MaxUsers = t.MaxUsers,
                EnabledModules = t.EnabledModules,
                CreatedAt = t.CreatedAt,
                UserCount = userCount,
                AdminUserId = adminUserId,
                AdminUserName = adminUserName
            });
        }

        return tenantDtos;
    }
}
