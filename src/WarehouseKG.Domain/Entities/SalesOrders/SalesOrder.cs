using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class SalesOrder : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public Guid? WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

    public string Currency { get; set; } = "KGS";

    public DateTime OrderDateUtc { get; set; }

    public DateTime? ExpectedDateUtc { get; set; }

    public DateTime? ConfirmedAtUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    public string? Notes { get; set; }

    public Guid? EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}
