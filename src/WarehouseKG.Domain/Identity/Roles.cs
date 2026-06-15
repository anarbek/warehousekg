namespace WarehouseKG.Domain.Identity;

/// <summary>
/// Canonical role names used across authentication, seeding, and authorization policies.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";

    public const string Manager = "Manager";

    public const string WarehouseOperator = "WarehouseOperator";

    public const string Viewer = "Viewer";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Admin,
        Manager,
        WarehouseOperator,
        Viewer
    };

    public static bool IsValid(string role) => All.Contains(role);
}
