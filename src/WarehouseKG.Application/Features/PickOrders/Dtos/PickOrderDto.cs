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

    public int LineCount { get; set; }
}
