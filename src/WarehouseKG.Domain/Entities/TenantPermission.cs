using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

/// <summary>
/// Tenant-scoped permission override for a role on a specific resource.
/// If no TenantPermission exists for a role+resource, the system default role hierarchy applies.
/// </summary>
public class TenantPermission : BaseEntity
{
    public Guid TenantId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanDelete { get; set; }
}
