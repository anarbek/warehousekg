using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class StockAudit : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public StockOperationStatus Status { get; set; } = StockOperationStatus.Draft;

    public DateTime? ReconciledAtUtc { get; set; }

    public string? Notes { get; set; }

    public ICollection<StockAuditLine> Lines { get; set; } = new List<StockAuditLine>();
}
