using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class StockReceipt : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public string? SupplierReference { get; set; }

    public StockOperationStatus Status { get; set; } = StockOperationStatus.Draft;

    public DateTime? ReceivedAtUtc { get; set; }

    public string? Notes { get; set; }

    public ICollection<StockReceiptLine> Lines { get; set; } = new List<StockReceiptLine>();
}
