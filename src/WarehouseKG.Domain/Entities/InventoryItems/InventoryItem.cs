using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Barcode { get; set; }

    public Guid CategoryId { get; set; }

    public ItemCategory? Category { get; set; }

    public Guid UnitOfMeasureId { get; set; }

    public UnitOfMeasure? UnitOfMeasure { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal ReorderLevel { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;
}
