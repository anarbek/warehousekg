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

    public static IServiceCollection AddWarehouseAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(RequireAdmin, policy =>
                policy.RequireRole(Roles.Admin))
            .AddPolicy(RequireManager, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager))
            .AddPolicy(RequireOperator, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager, Roles.WarehouseOperator))
            .AddPolicy(RequireViewer, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager, Roles.WarehouseOperator, Roles.Viewer));

        return services;
    }
}
