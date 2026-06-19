using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class Employee : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }

    public Guid? PositionId { get; set; }
    public Position? Position { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>Optional link to ASP.NET Identity user account.</summary>
    public Guid? ApplicationUserId { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeShiftAssignment> ShiftAssignments { get; set; } = new List<EmployeeShiftAssignment>();
    public ICollection<EmployeeWarehouseAssignment> WarehouseAssignments { get; set; } = new List<EmployeeWarehouseAssignment>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
