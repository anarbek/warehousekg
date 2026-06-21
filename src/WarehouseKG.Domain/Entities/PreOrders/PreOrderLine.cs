using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class PreOrderLine : BaseEntity
{
    public Guid PreOrderId { get; set; }

    public PreOrder? PreOrder { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal WarehouseStockSnapshot { get; set; }

    public decimal StockDifference { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal LineTotal { get; set; }
}
