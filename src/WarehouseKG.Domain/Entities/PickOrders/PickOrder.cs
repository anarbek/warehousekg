using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class PickOrder : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public string? Reference { get; set; }

    public StockOperationStatus Status { get; set; } = StockOperationStatus.Draft;

    public DateTime? PickedAtUtc { get; set; }

    public DateTime? PlannedPickDate { get; set; }

    public string? Notes { get; set; }

    public Guid? EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public ICollection<PickOrderLine> Lines { get; set; } = new List<PickOrderLine>();
}
