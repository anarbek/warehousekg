using WarehouseKG.Application.Common.Models;

namespace WarehouseKG.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the identity store (ASP.NET Core Identity in Infrastructure) used by the
/// auth command handlers. Keeps the Application layer free of Identity dependencies.
/// </summary>
public interface IIdentityService
{
    Task<IdentityCreationResult> CreateUserAsync(
        string userName,
        string? email,
        string password,
        Guid tenantId,
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the user when the username/password pair is valid; otherwise <c>null</c>.</summary>
    Task<AuthUser?> ValidateCredentialsAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an existing user by their external-login key (e.g. Google sub), or creates one
    /// if no matching account exists.  The external login is linked to the user account.
    /// </summary>
    Task<AuthUser> FindOrCreateByExternalLoginAsync(
        string provider,
        string providerKey,
        string? email,
        string? displayName,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the number of users belonging to a specific tenant.</summary>
    Task<int> GetUserCountForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the ID of the first Admin user in a tenant, or null.</summary>
    Task<Guid?> GetAdminUserIdForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the username of the first Admin user in a tenant, or null.</summary>
    Task<string?> GetAdminUserNameForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
