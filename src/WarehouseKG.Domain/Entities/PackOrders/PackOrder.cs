using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class PackOrder : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public Guid? PickOrderId { get; set; }

    public PickOrder? PickOrder { get; set; }

    public StockOperationStatus Status { get; set; } = StockOperationStatus.Draft;

    public DateTime? PackedAtUtc { get; set; }

    public DateTime? ActualPackDate { get; set; }

    public string? Notes { get; set; }

    public Guid? EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public ICollection<PackOrderLine> Lines { get; set; } = new List<PackOrderLine>();
}
