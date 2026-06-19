using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class VehicleType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DefaultCapacityKg { get; set; }
    public decimal? DefaultCapacityM3 { get; set; }
    public bool IsActive { get; set; } = true;
}
