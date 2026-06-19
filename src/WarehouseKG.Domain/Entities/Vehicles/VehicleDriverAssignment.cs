using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class VehicleDriverAssignment : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime AssignedFromUtc { get; set; }
    public DateTime? AssignedToUtc { get; set; }
    public bool IsPrimary { get; set; }
}
