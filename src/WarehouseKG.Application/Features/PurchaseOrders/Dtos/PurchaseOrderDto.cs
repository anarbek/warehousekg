using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PurchaseOrders.Dtos;

public class PurchaseOrderDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public Guid? WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public PurchaseOrderStatus Status { get; set; }

    public string Currency { get; set; } = "KGS";

    public DateTime OrderDateUtc { get; set; }

    public DateTime? ExpectedDateUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }

    public string? Notes { get; set; }

    public decimal TotalAmount { get; set; }

    public List<PurchaseOrderLineDto> Lines { get; set; } = new();
}

public class PurchaseOrderLineDto
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}

public class PurchaseOrderSummaryDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public PurchaseOrderStatus Status { get; set; }

    public string Currency { get; set; } = "KGS";

    public DateTime OrderDateUtc { get; set; }

    public decimal TotalAmount { get; set; }

    public int LineCount { get; set; }
}
