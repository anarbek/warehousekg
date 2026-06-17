using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Api.Authorization;

public class TenantPermissionRequirement : IAuthorizationRequirement
{
    public string Resource { get; }
    public PermissionType Type { get; }

    public TenantPermissionRequirement(string resource, PermissionType type)
    {
        Resource = resource;
        Type = type;
    }
}

public enum PermissionType { Read, Write }

public class TenantPermissionHandler : AuthorizationHandler<TenantPermissionRequirement>
{
    private readonly IApplicationDbContext _context;

    public TenantPermissionHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantPermissionRequirement requirement)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim)) return;

        var tenantId = Guid.Parse(tenantIdClaim);
        var roles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        // Admin bypasses all permission checks
        if (roles.Contains(Roles.Admin))
        {
            context.Succeed(requirement);
            return;
        }

        // Check tenant-scoped overrides
        foreach (var role in roles)
        {
            var permission = await _context.TenantPermissions
                .FirstOrDefaultAsync(p =>
                    p.TenantId == tenantId &&
                    p.RoleName == role &&
                    p.Resource == requirement.Resource);

            if (permission != null)
            {
                var allowed = requirement.Type == PermissionType.Read
                    ? permission.CanRead
                    : permission.CanWrite;

                if (allowed) context.Succeed(requirement);
                return; // Explicit permission exists — don't fall through
            }
        }

        // Fallback: Viewer can read, Manager/Operator can write
        if (requirement.Type == PermissionType.Read && roles.Count > 0)
        {
            context.Succeed(requirement);
            return;
        }

        if (requirement.Type == PermissionType.Write &&
            roles.Any(r => r is Roles.Manager or Roles.WarehouseOperator))
        {
            context.Succeed(requirement);
        }
    }
}
