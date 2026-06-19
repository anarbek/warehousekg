using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class EmployeeShiftAssignment : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public Guid ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
}
