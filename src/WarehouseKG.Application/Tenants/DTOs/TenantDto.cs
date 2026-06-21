namespace WarehouseKG.Application.Tenants.DTOs;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string DefaultCurrency { get; set; } = "KGS";
    public int Status { get; set; }
    public int? MaxUsers { get; set; }
    public string? EnabledModules { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public Guid? AdminUserId { get; set; }
    public string? AdminUserName { get; set; }
}
