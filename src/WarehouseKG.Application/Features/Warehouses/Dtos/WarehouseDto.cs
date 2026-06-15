namespace WarehouseKG.Application.Features.Warehouses.Dtos;

public class WarehouseDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
