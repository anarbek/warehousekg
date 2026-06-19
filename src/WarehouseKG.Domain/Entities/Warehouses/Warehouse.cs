using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
    public ICollection<EmployeeWarehouseAssignment> EmployeeAssignments { get; set; } = new List<EmployeeWarehouseAssignment>();
}
