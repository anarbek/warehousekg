using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Common.Models;
using WarehouseKG.Application.Features.Auth.Dtos;

namespace WarehouseKG.Application.Features.Auth.Commands;

/// <summary>
/// Shared logic for minting an access token + a freshly persisted refresh token for a user.
/// Used by login, register, and refresh handlers so token issuance stays consistent.
/// </summary>
internal static class AuthTokenIssuer
{
    public static async Task<AuthResponse> IssueAsync(
        AuthUser user,
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore,
        CancellationToken cancellationToken)
    {
        var accessToken = tokenGenerator.CreateAccessToken(user);
        var refreshToken = tokenGenerator.CreateRefreshToken();

        await refreshTokenStore.CreateAsync(
            user.Id,
            user.TenantId,
            refreshToken,
            cancellationToken);

        return new AuthResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.TenantId,
            user.Roles,
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken);
    }
}
