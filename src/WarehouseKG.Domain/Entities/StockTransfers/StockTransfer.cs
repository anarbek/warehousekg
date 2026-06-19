using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class StockTransfer : BaseEntity
{
    public string Number { get; set; } = string.Empty;

    public Guid SourceWarehouseId { get; set; }

    public Warehouse? SourceWarehouse { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public Warehouse? DestinationWarehouse { get; set; }

    public StockOperationStatus Status { get; set; } = StockOperationStatus.Draft;

    public DateTime? TransferredAtUtc { get; set; }

    public string? Notes { get; set; }

    public Guid? EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public ICollection<StockTransferLine> Lines { get; set; } = new List<StockTransferLine>();
}
