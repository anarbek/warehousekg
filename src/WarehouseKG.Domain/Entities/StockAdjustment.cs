using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class StockAdjustment : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public StockAdjustmentReason Reason { get; set; } = StockAdjustmentReason.Correction;

    public StockOperationStatus Status { get; set; } = StockOperationStatus.Draft;

    public DateTime? AdjustedAtUtc { get; set; }

    public string? Notes { get; set; }

    public ICollection<StockAdjustmentLine> Lines { get; set; } = new List<StockAdjustmentLine>();
}
