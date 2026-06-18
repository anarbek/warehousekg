using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PickOrders.Dtos;

public class PickOrderDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public string? Reference { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? PickedAtUtc { get; set; }

    public DateTime? PlannedPickDate { get; set; }

    public string? Notes { get; set; }

    public List<PickOrderLineDto> Lines { get; set; } = new();
}

public class PickOrderLineDto
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public Guid? WarehouseLocationId { get; set; }

    public decimal Quantity { get; set; }
}

public class PickOrderSummaryDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public string? Reference { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? PickedAtUtc { get; set; }

    public DateTime? PlannedPickDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public int LineCount { get; set; }
}
