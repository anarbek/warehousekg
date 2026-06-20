using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Dispatching.Routes.Queries;

public class RouteDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleCode { get; set; }
    public Guid? DriverEmployeeId { get; set; }
    public string? DriverName { get; set; }
    public RouteStatus Status { get; set; }
    public string? Notes { get; set; }
    public int StopCount { get; set; }
}

public class RouteDetailDto : RouteDto
{
    public List<StopDto> Stops { get; set; } = new();
}

public class StopDto
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public int SequenceNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? PlannedArrivalUtc { get; set; }
    public DateTime? PlannedDepartureUtc { get; set; }
    public DateTime? ActualArrivalUtc { get; set; }
    public DateTime? ActualDepartureUtc { get; set; }
    public StopStatus Status { get; set; }
    public bool HasRegulatedGoods { get; set; }
    public string? Notes { get; set; }
    public int ShipmentCount { get; set; }
    public List<ShipmentDto> Shipments { get; set; } = new();
}

public class ShipmentDto
{
    public Guid Id { get; set; }
    public Guid DeliveryStopId { get; set; }
    public Guid SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public StopStatus Status { get; set; }
}
