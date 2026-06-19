using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class ItemCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
}
