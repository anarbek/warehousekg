namespace WarehouseKG.Application.Features.Customers.Dtos;

public class CustomerDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? TaxId { get; set; }

    public bool IsActive { get; set; }
}
