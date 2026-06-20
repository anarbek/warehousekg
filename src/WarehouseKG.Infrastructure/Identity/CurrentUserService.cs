using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("unique_name")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    public Guid? UserId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return raw != null && Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public Guid? EmployeeId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtTokenGenerator.EmployeeIdClaimType)?.Value;
            return raw != null && Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}
