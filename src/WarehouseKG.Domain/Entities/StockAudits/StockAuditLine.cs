using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class StockAuditLine : BaseEntity
{
    public Guid StockAuditId { get; set; }

    public StockAudit? StockAudit { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    // Stock on hand captured when the audit line was created (the "book" figure).
    public decimal SystemQuantity { get; set; }

    // Physically counted quantity. Variance (Counted - System) is computed in read models.
    public decimal CountedQuantity { get; set; }
}
