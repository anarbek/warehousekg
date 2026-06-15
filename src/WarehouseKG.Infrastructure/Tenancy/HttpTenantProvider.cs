using Microsoft.AspNetCore.Http;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Infrastructure.Tenancy;

public class HttpTenantProvider : ITenantProvider
{
    public const string TenantHeaderName = "X-Tenant-Id";

    // Mirrors JwtTokenGenerator.TenantIdClaimType; duplicated here to avoid a token-layer dependency.
    public const string TenantClaimType = "tenant_id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return Guid.Empty;
        }

        // Explicit header wins (supports tooling / service-to-service calls).
        if (httpContext.Request.Headers.TryGetValue(TenantHeaderName, out var headerValue) &&
            Guid.TryParse(headerValue.ToString(), out var headerTenantId))
        {
            return headerTenantId;
        }

        // Fall back to the tenant baked into the authenticated user's JWT.
        var claimValue = httpContext.User?.FindFirst(TenantClaimType)?.Value;
        if (Guid.TryParse(claimValue, out var claimTenantId))
        {
            return claimTenantId;
        }

        // TODO: resolve tenant from subdomain once host-based routing is in place.
        return Guid.Empty;
    }
}
