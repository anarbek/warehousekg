using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class Shift : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeShiftAssignment> EmployeeAssignments { get; set; } = new List<EmployeeShiftAssignment>();
}
