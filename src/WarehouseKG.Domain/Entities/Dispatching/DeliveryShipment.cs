using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class DeliveryShipment : BaseEntity
{
    public Guid DeliveryStopId { get; set; }
    public DeliveryStop? DeliveryStop { get; set; }

    public Guid SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public StopStatus Status { get; set; } = StopStatus.Pending;
}
