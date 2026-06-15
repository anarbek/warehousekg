using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class StockReceiptLine : BaseEntity
{
    public Guid StockReceiptId { get; set; }

    public StockReceipt? StockReceipt { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public Guid? WarehouseLocationId { get; set; }

    public WarehouseLocation? WarehouseLocation { get; set; }

    public decimal Quantity { get; set; }
}
