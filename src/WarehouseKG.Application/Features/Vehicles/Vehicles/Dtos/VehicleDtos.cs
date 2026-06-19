namespace WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

public class VehicleDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string? VIN { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int? ManufactureYear { get; set; }
    public Guid? VehicleTypeId { get; set; }
    public string? VehicleTypeName { get; set; }
    public string OwnershipType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal? FuelConsumptionRate { get; set; }
    public decimal? MaxCapacityKg { get; set; }
    public decimal? MaxCapacityM3 { get; set; }
    public decimal CurrentMileageKm { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceProvider { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? TechInspectionExpiryDate { get; set; }
    public decimal? NextMaintenanceMileageKm { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public bool HasGpsTracker { get; set; }
    public string? Notes { get; set; }
    public string? AssignedDriverName { get; set; }
}

public class VehicleDetailDto : VehicleDto
{
    public List<DriverAssignmentDto> DriverAssignments { get; set; } = new();
    public List<MaintenanceRecordDto> MaintenanceRecords { get; set; } = new();
    public List<InsuranceRecordDto> InsuranceRecords { get; set; } = new();
    public List<InspectionRecordDto> InspectionRecords { get; set; } = new();
}

public class DriverAssignmentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime AssignedFromUtc { get; set; }
    public DateTime? AssignedToUtc { get; set; }
    public bool IsPrimary { get; set; }
}

public class MaintenanceRecordDto
{
    public Guid Id { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal MileageKm { get; set; }
    public decimal Cost { get; set; }
    public string? Description { get; set; }
    public string? ServiceProvider { get; set; }
    public string? Notes { get; set; }
    public decimal? NextDueMileageKm { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string? VehicleCode { get; set; }
    public string? VehiclePlate { get; set; }
}

public class InsuranceRecordDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? CoverageType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public string? Description { get; set; }
    public string? VehicleCode { get; set; }
    public string? VehiclePlate { get; set; }
}

public class InspectionRecordDto
{
    public Guid Id { get; set; }
    public DateTime InspectionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Inspector { get; set; }
    public string? Notes { get; set; }
    public string? VehicleCode { get; set; }
    public string? VehiclePlate { get; set; }
}
