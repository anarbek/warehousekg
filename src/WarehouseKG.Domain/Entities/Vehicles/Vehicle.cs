using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class Vehicle : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string? VIN { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int? ManufactureYear { get; set; }

    public Guid? VehicleTypeId { get; set; }
    public VehicleType? VehicleType { get; set; }

    public VehicleOwnershipType OwnershipType { get; set; } = VehicleOwnershipType.Owned;
    public VehicleStatus Status { get; set; } = VehicleStatus.Active;
    public FuelType FuelType { get; set; } = FuelType.Diesel;
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

    public ICollection<VehicleDriverAssignment> DriverAssignments { get; set; } = new List<VehicleDriverAssignment>();
    public ICollection<VehicleMaintenanceRecord> MaintenanceRecords { get; set; } = new List<VehicleMaintenanceRecord>();
    public ICollection<VehicleInsuranceRecord> InsuranceRecords { get; set; } = new List<VehicleInsuranceRecord>();
    public ICollection<VehicleInspectionRecord> InspectionRecords { get; set; } = new List<VehicleInspectionRecord>();
    public ICollection<VehicleFuelRecord> FuelRecords { get; set; } = new List<VehicleFuelRecord>();
}
