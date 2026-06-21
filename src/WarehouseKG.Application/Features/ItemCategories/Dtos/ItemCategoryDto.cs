namespace WarehouseKG.Application.Features.ItemCategories.Dtos;

public class ItemCategoryDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool RequiresAgeVerification { get; set; }

    public Guid? ParentId { get; set; }

    public string? ParentName { get; set; }

    public IReadOnlyList<ItemCategoryDto> Children { get; set; } = Array.Empty<ItemCategoryDto>();
}
