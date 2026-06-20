using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class DeliveryStop : BaseEntity
{
    public Guid RouteId { get; set; }
    public DeliveryRoute? Route { get; set; }

    public int SequenceNumber { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string Address { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime? PlannedArrivalUtc { get; set; }
    public DateTime? PlannedDepartureUtc { get; set; }
    public DateTime? ActualArrivalUtc { get; set; }
    public DateTime? ActualDepartureUtc { get; set; }

    public StopStatus Status { get; set; } = StopStatus.Pending;

    public bool HasRegulatedGoods { get; set; }

    public string? Notes { get; set; }

    public ICollection<DeliveryShipment> Shipments { get; set; } = new List<DeliveryShipment>();
}
