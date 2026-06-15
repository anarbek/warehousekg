using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Infrastructure.Persistence;

namespace WarehouseKG.Infrastructure.Identity;

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly WarehouseKgDbContext _context;
    private readonly JwtOptions _options;

    public RefreshTokenStore(WarehouseKgDbContext context, IOptions<JwtOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task CreateAsync(
        Guid userId,
        Guid tenantId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            Token = token,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_options.RefreshTokenDays)
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid?> ConsumeAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return null;
        }

        refreshToken.RevokedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return refreshToken.UserId;
    }
}
