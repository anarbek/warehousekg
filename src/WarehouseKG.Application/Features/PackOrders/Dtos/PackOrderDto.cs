using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.PackOrders.Dtos;

public class PackOrderDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public Guid? PickOrderId { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? PackedAtUtc { get; set; }

    public DateTime? ActualPackDate { get; set; }

    public string? Notes { get; set; }

    public List<PackOrderLineDto> Lines { get; set; } = new();
}

public class PackOrderLineDto
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal Quantity { get; set; }

    public string? PackageLabel { get; set; }
}

public class PackOrderSummaryDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? PackedAtUtc { get; set; }

    public DateTime? ActualPackDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public int LineCount { get; set; }
}
