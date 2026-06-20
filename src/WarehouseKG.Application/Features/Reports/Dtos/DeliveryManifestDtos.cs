namespace WarehouseKG.Application.Features.Reports.Dtos;

public class DeliveryManifestDto
{
    public Guid RouteId { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? VehicleCode { get; set; }
    public string? DriverName { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ManifestStopDto> Stops { get; set; } = new();
}

public class ManifestStopDto
{
    public int SequenceNumber { get; set; }
    public string? CustomerName { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ManifestShipmentDto> Shipments { get; set; } = new();
}

public class ManifestShipmentDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public List<ManifestLineDto> Lines { get; set; } = new();
}

public class ManifestLineDto
{
    public string ItemName { get; set; } = string.Empty;
    public string ItemSku { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
