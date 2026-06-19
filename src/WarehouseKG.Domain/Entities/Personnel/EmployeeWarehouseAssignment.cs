using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class EmployeeWarehouseAssignment : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public bool IsPrimary { get; set; }
}
