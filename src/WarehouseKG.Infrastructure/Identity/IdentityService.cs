using Microsoft.AspNetCore.Identity;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Common.Models;

namespace WarehouseKG.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityCreationResult> CreateUserAsync(
        string userName,
        string? email,
        string password,
        Guid tenantId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByNameAsync(userName);
        if (existing is not null)
        {
            return IdentityCreationResult.Failure(new[] { "A user with this username already exists." });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            TenantId = tenantId
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return IdentityCreationResult.Failure(createResult.Errors.Select(e => e.Description).ToArray());
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            return IdentityCreationResult.Failure(roleResult.Errors.Select(e => e.Description).ToArray());
        }

        return IdentityCreationResult.Success(await ToAuthUserAsync(user));
    }

    public async Task<AuthUser?> ValidateCredentialsAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return null;
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return null;
        }

        return await ToAuthUserAsync(user);
    }

    public async Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : await ToAuthUserAsync(user);
    }

    private async Task<AuthUser> ToAuthUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AuthUser(user.Id, user.UserName ?? string.Empty, user.Email, user.TenantId, roles.ToList());
    }
}
