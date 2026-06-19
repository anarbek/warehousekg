using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class VehicleFuelRecord : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime Date { get; set; }
    public decimal Liters { get; set; }
    public decimal Cost { get; set; }
    public decimal MileageKm { get; set; }
    public FuelType FuelType { get; set; }
    public string? Station { get; set; }
    public string? Notes { get; set; }
}
