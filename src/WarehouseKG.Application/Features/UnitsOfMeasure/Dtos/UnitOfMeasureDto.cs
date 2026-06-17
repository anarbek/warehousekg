namespace WarehouseKG.Application.Features.UnitsOfMeasure.Dtos;

public class UnitOfMeasureDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}
