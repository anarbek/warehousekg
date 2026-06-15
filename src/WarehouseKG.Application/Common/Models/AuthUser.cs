namespace WarehouseKG.Application.Common.Models;

/// <summary>
/// Provider-agnostic snapshot of an authenticated user, surfaced by <c>IIdentityService</c>
/// so the Application layer never depends on ASP.NET Core Identity types directly.
/// </summary>
public sealed record AuthUser(
    Guid Id,
    string UserName,
    string? Email,
    Guid TenantId,
    IReadOnlyList<string> Roles);
