using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Common.Models;
using WarehouseKG.Domain.Identity;

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

    public async Task<AuthUser> FindOrCreateByExternalLoginAsync(
        string provider,
        string providerKey,
        string? email,
        string? displayName,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // 1. Try to find by the external login link (e.g. Google sub).
        var user = await _userManager.FindByLoginAsync(provider, providerKey);
        if (user is not null)
        {
            return await ToAuthUserAsync(user);
        }

        // 2. If email is known, link to an existing local account instead of creating a duplicate.
        if (!string.IsNullOrWhiteSpace(email))
        {
            user = await _userManager.FindByEmailAsync(email);
        }

        // 3. Create a new account if no match was found.
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = !string.IsNullOrWhiteSpace(email)
                    ? email
                    : $"{provider.ToLowerInvariant()}_{providerKey}",
                Email = email,
                EmailConfirmed = true,
                TenantId = tenantId
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create Google user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, Roles.Viewer);
        }

        // 4. Persist the external login link so future sign-ins hit path 1.
        var loginInfo = new UserLoginInfo(provider, providerKey, displayName ?? provider);
        await _userManager.AddLoginAsync(user, loginInfo);

        return await ToAuthUserAsync(user);
    }

    private async Task<AuthUser> ToAuthUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AuthUser(user.Id, user.UserName ?? string.Empty, user.Email, user.TenantId, roles.ToList(), user.EmployeeId);
    }

    public async Task<int> GetUserCountForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users.CountAsync(u => u.TenantId == tenantId, cancellationToken);
    }

    public async Task<Guid?> GetAdminUserIdForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        return admins.FirstOrDefault(u => u.TenantId == tenantId)?.Id;
    }

    public async Task<string?> GetAdminUserNameForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        return admins.FirstOrDefault(u => u.TenantId == tenantId)?.UserName;
    }
}
