using WarehouseKG.Application.Common.Models;

namespace WarehouseKG.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>Issues a signed JWT access token carrying the user's id, tenant, and role claims.</summary>
    GeneratedAccessToken CreateAccessToken(AuthUser user);

    /// <summary>Generates a cryptographically random, opaque refresh token.</summary>
    string CreateRefreshToken();
}
