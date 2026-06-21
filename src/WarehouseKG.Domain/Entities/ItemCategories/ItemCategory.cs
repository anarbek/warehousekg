using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class ItemCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates that items in this category require age verification upon delivery
    /// (e.g. alcohol, tobacco products).
    /// </summary>
    public bool RequiresAgeVerification { get; set; }

    /// <summary>Parent category for hierarchical tree structure.</summary>
    public Guid? ParentId { get; set; }

    public ItemCategory? Parent { get; set; }

    public ICollection<ItemCategory> Children { get; set; } = new List<ItemCategory>();

    public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
}
