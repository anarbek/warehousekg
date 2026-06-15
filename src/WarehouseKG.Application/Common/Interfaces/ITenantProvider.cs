namespace WarehouseKG.Application.Common.Interfaces;

public interface ITenantProvider
{
    Guid GetTenantId();
}
