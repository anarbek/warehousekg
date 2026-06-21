using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class Customer : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? TaxId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}
