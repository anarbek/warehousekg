using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockAdjustments.Dtos;

public class StockAdjustmentDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public StockAdjustmentReason Reason { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? AdjustedAtUtc { get; set; }

    public string? Notes { get; set; }

    public Guid? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public List<StockAdjustmentLineDto> Lines { get; set; } = new();
}

public class StockAdjustmentLineDto
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal QuantityChange { get; set; }

    public string? Notes { get; set; }
}

public class StockAdjustmentSummaryDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public StockAdjustmentReason Reason { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? AdjustedAtUtc { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public int LineCount { get; set; }
}
