using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class PackOrderLine : BaseEntity
{
    public Guid PackOrderId { get; set; }

    public PackOrder? PackOrder { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public decimal Quantity { get; set; }

    public string? PackageLabel { get; set; }
}
