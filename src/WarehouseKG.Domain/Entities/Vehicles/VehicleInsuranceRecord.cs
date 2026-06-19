using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class VehicleInsuranceRecord : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? CoverageType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public string? Description { get; set; }
}
