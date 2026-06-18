using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    public Guid? WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    public string Currency { get; set; } = "KGS";

    public DateTime OrderDateUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }

    public string? Notes { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
