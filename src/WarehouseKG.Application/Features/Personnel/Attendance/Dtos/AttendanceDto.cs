using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Personnel.Attendance.Dtos;

public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public DateTime Date { get; set; }
    public DateTime? ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
}
