using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Domain.Entities;

public class VehicleInspectionRecord : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime InspectionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public InspectionResult Result { get; set; }
    public string? Inspector { get; set; }
    public string? Notes { get; set; }
}
