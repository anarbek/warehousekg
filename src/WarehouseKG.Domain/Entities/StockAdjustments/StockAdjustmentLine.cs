using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class StockAdjustmentLine : BaseEntity
{
    public Guid StockAdjustmentId { get; set; }

    public StockAdjustment? StockAdjustment { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    // Signed delta applied to QuantityOnHand when the adjustment is completed
    // (positive increases stock, negative decreases it).
    public decimal QuantityChange { get; set; }

    public string? Notes { get; set; }
}
