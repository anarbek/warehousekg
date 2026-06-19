namespace WarehouseKG.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserName { get; }
    Guid? UserId { get; }
}
