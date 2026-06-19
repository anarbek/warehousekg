using Microsoft.AspNetCore.Authorization;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Api.Authorization;

/// <summary>
/// Named role-based authorization policies. Higher roles inherit lower-privilege access
/// (e.g. an Admin satisfies every policy).
/// </summary>
public static class AuthorizationPolicies
{
    public const string RequireAdmin = "RequireAdmin";

    public const string RequireManager = "RequireManager";

    public const string RequireOperator = "RequireOperator";

    public const string RequireViewer = "RequireViewer";

    public const string ReadPolicy = "read";
    public const string WritePolicy = "write";

    public static IServiceCollection AddWarehouseAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, TenantPermissionHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(RequireAdmin, policy =>
                policy.RequireRole(Roles.Admin))
            .AddPolicy(RequireManager, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager))
            .AddPolicy(RequireOperator, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager, Roles.WarehouseOperator))
            .AddPolicy(RequireViewer, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager, Roles.WarehouseOperator, Roles.Auditor, Roles.Dispatcher, Roles.HR, Roles.Viewer));

        // Resource-based policies for tenant-scoped permission overrides
        foreach (var resource in Resources.All)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy($"{resource}:read", policy =>
                    policy.AddRequirements(new TenantPermissionRequirement(resource, PermissionType.Read)))
                .AddPolicy($"{resource}:write", policy =>
                    policy.AddRequirements(new TenantPermissionRequirement(resource, PermissionType.Write)))
                .AddPolicy($"{resource}:delete", policy =>
                    policy.AddRequirements(new TenantPermissionRequirement(resource, PermissionType.Delete)));
        }

        return services;
    }
}
