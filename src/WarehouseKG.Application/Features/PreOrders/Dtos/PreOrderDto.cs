using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PreOrders.Dtos;

public class PreOrderDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? PresellerId { get; set; }
    public string? PresellerName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public PreOrderStatus Status { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string Currency { get; set; } = "KGS";
    public DateTime OrderDateUtc { get; set; }
    public DateTime? ExpectedDateUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? RejectedAtUtc { get; set; }
    public DateTime? ConvertedAtUtc { get; set; }
    public Guid? ConvertedSalesOrderId { get; set; }
    public string? ConvertedSalesOrderNumber { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PreOrderLineDto> Lines { get; set; } = new();
}

public class PreOrderLineDto
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public string? InventoryItemName { get; set; }
    public string? Sku { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal WarehouseStockSnapshot { get; set; }
    public decimal StockDifference { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
}

public class PreOrderSummaryDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? PresellerName { get; set; }
    public PreOrderStatus Status { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int LineCount { get; set; }
    public DateTime OrderDateUtc { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PreOrderLineInput
{
    public Guid InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
}

public class PaymentTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class PreOrderWarehouseStockDto
{
    public Guid InventoryItemId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
}
