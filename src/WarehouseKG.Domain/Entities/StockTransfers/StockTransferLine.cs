using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class StockTransferLine : BaseEntity
{
    public Guid StockTransferId { get; set; }

    public StockTransfer? StockTransfer { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public decimal Quantity { get; set; }
}
