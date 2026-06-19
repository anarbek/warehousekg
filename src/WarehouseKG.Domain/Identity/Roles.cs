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

    public const string Auditor = "Auditor";

    public const string Dispatcher = "Dispatcher";

    public const string HR = "HR";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Admin,
        Manager,
        Auditor,
        Dispatcher,
        WarehouseOperator,
        HR,
        Viewer
    };

    public static bool IsValid(string role) => All.Contains(role);
}
