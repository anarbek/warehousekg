using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class VehicleMaintenanceRecord : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public MaintenanceType MaintenanceType { get; set; }
    public DateTime Date { get; set; }
    public decimal MileageKm { get; set; }
    public decimal Cost { get; set; }
    public string? Description { get; set; }
    public string? ServiceProvider { get; set; }
    public string? Notes { get; set; }

    public decimal? NextDueMileageKm { get; set; }
    public DateTime? NextDueDate { get; set; }
}
