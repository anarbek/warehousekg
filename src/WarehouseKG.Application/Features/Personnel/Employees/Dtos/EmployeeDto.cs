namespace WarehouseKG.Application.Features.Personnel.Employees.Dtos;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public Guid? PositionId { get; set; }
    public string? PositionName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? ApplicationUserId { get; set; }
    public string? LinkedUserName { get; set; }
    public bool IsActive { get; set; }
}
