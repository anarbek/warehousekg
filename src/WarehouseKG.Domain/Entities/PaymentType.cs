using WarehouseKG.Domain.Common;

namespace WarehouseKG.Domain.Entities;

public class PaymentType : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
