using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; set; }

    public SalesOrder? SalesOrder { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
