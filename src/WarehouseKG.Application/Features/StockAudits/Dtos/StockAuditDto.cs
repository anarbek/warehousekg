using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.StockAudits.Dtos;

public class StockAuditDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? ReconciledAtUtc { get; set; }

    public string? Notes { get; set; }

    public List<StockAuditLineDto> Lines { get; set; } = new();
}

public class StockAuditLineDto
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal CountedQuantity { get; set; }

    public decimal Variance { get; set; }
}

public class StockAuditSummaryDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public StockOperationStatus Status { get; set; }

    public DateTime? ReconciledAtUtc { get; set; }

    public int LineCount { get; set; }

    public decimal TotalVariance { get; set; }
}
