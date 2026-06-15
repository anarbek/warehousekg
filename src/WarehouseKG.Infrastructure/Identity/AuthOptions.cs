namespace WarehouseKG.Infrastructure.Identity;

/// <summary>Bound from the <c>Jwt</c> configuration section.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "WarehouseKG";

    public string Audience { get; set; } = "WarehouseKG";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 7;
}

/// <summary>Bound from the <c>Authentication:Google</c> configuration section (OAuth, future use).</summary>
public class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}
