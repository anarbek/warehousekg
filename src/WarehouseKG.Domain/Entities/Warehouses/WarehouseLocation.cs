using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class WarehouseLocation : BaseEntity
{
    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Zone { get; set; }

    public string? Aisle { get; set; }

    public string? Bin { get; set; }

    public bool IsActive { get; set; } = true;
}
