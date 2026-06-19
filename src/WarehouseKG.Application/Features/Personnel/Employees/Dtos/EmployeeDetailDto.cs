namespace WarehouseKG.Application.Features.Personnel.Employees.Dtos;

public class EmployeeDetailDto : EmployeeDto
{
    public List<ShiftAssignmentDto> ShiftAssignments { get; set; } = new();
    public List<WarehouseAssignmentDto> WarehouseAssignments { get; set; } = new();
}

public class ShiftAssignmentDto
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
}

public class WarehouseAssignmentDto
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
