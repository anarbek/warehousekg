using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockTransfers.Dtos;

public class StockTransferDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid SourceWarehouseId { get; set; }

    public string? SourceWarehouseName { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public string? DestinationWarehouseName { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? TransferredAtUtc { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Notes { get; set; }

    public List<StockTransferLineDto> Lines { get; set; } = new();
}

public class StockTransferLineDto
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal Quantity { get; set; }
}

public class StockTransferSummaryDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid SourceWarehouseId { get; set; }

    public string? SourceWarehouseName { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public string? DestinationWarehouseName { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? TransferredAtUtc { get; set; }

    public DateTime CreatedAt { get; set; }

    public int LineCount { get; set; }
}
