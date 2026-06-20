using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class DeliveryRoute : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid? DriverEmployeeId { get; set; }
    public Employee? DriverEmployee { get; set; }

    public RouteStatus Status { get; set; } = RouteStatus.Planned;

    public string? Notes { get; set; }

    public ICollection<DeliveryStop> Stops { get; set; } = new List<DeliveryStop>();
}
