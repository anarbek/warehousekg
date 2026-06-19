using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class PurchaseOrderLine : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
