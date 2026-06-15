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
}
