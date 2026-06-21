using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

/// <summary>
/// Represents a tenant organization in the multi-tenant warehouse system.
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>Organization name displayed in UI</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Unique slug for subdomain-based resolution (future)</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Tenant's contact email</summary>
    public string? ContactEmail { get; set; }

    /// <summary>Tenant's contact phone</summary>
    public string? ContactPhone { get; set; }

    /// <summary>ISO currency code for this tenant (default: KGS)</summary>
    public string DefaultCurrency { get; set; } = "KGS";

    /// <summary>Tenant subscription status</summary>
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    /// <summary>Max number of users allowed (for future billing)</summary>
    public int? MaxUsers { get; set; }

    /// <summary>Features/modules enabled for this tenant (JSON array or comma-separated)</summary>
    public string? EnabledModules { get; set; }
}
