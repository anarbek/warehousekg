using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class PreOrder : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public Guid? PresellerId { get; set; }

    public Employee? Preseller { get; set; }

    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public PreOrderStatus Status { get; set; } = PreOrderStatus.Draft;

    public string PaymentType { get; set; } = string.Empty;

    public string Currency { get; set; } = "KGS";

    public DateTime OrderDateUtc { get; set; }

    public DateTime? ExpectedDateUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    public DateTime? RejectedAtUtc { get; set; }

    public DateTime? ConvertedAtUtc { get; set; }

    public Guid? ConvertedSalesOrderId { get; set; }

    public SalesOrder? ConvertedSalesOrder { get; set; }

    public string? Notes { get; set; }

    public decimal TotalAmount { get; set; }

    public ICollection<PreOrderLine> Lines { get; set; } = new List<PreOrderLine>();
}
