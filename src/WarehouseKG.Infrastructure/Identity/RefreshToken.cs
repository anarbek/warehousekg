namespace WarehouseKG.Infrastructure.Identity;

/// <summary>
/// Persisted, single-use refresh token. Intentionally not a tenant-filtered <c>BaseEntity</c>: the
/// refresh endpoint is called with only the token (no tenant header), so lookups must not be filtered.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
