using Microsoft.AspNetCore.Identity;

namespace WarehouseKG.Infrastructure.Identity;

/// <summary>
/// Application user backed by ASP.NET Core Identity, extended with the owning tenant.
/// Roles are managed through Identity's role store (see <c>Roles</c>).
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }

    /// <summary>Links the login user to an Employee record for driver/dispatcher workflows.</summary>
    public Guid? EmployeeId { get; set; }
}
