namespace WarehouseKG.Application.Common.Interfaces;

/// <summary>
/// Persists and validates opaque refresh tokens. Tokens are single-use: <see cref="ConsumeAsync"/>
/// revokes the presented token so the caller can rotate it for a fresh pair.
/// </summary>
public interface IRefreshTokenStore
{
    /// <summary>Persists a new refresh token for the user; expiry is derived from configured lifetime.</summary>
    Task CreateAsync(
        Guid userId,
        Guid tenantId,
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the token (exists, not expired, not already revoked), marks it revoked, and returns
    /// the owning user id. Returns <c>null</c> when the token is invalid.
    /// </summary>
    Task<Guid?> ConsumeAsync(string token, CancellationToken cancellationToken = default);
}
