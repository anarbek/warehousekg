using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockReceipts.Dtos;

public class StockReceiptDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public string? SupplierReference { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Notes { get; set; }

    public List<StockReceiptLineDto> Lines { get; set; } = new();
}

public class StockReceiptLineDto
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public Guid? WarehouseLocationId { get; set; }

    public decimal Quantity { get; set; }
}

public class StockReceiptSummaryDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public string? SupplierReference { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }

    public DateTime CreatedAt { get; set; }

    public int LineCount { get; set; }
}
