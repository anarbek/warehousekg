using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.SalesOrders.Dtos;

public class SalesOrderDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public Guid? WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public SalesOrderStatus Status { get; set; }

    public string Currency { get; set; } = "KGS";

    public DateTime OrderDateUtc { get; set; }

    public DateTime? ExpectedDateUtc { get; set; }

    public DateTime? ConfirmedAtUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    public string? Notes { get; set; }

    public decimal TotalAmount { get; set; }

    public Guid? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public List<SalesOrderLineDto> Lines { get; set; } = new();
}

public class SalesOrderLineDto
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}

public class SalesOrderSummaryDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public SalesOrderStatus Status { get; set; }

    public string Currency { get; set; } = "KGS";

    public DateTime OrderDateUtc { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpectedDateUtc { get; set; }

    public decimal TotalAmount { get; set; }

    public Guid? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public int LineCount { get; set; }
}
