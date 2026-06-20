using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;

namespace WarehouseKG.Application.Features.Dispatching.Routes.Queries;

public record GetDeliveryRoutesQuery : IRequest<IReadOnlyList<RouteDto>>;

public class GetDeliveryRoutesQueryHandler : IRequestHandler<GetDeliveryRoutesQuery, IReadOnlyList<RouteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeliveryRoutesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<RouteDto>> Handle(GetDeliveryRoutesQuery request, CancellationToken ct)
    {
        return await _context.DeliveryRoutes
            .AsNoTracking()
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.Code)
            .Select(r => new RouteDto
            {
                Id = r.Id,
                Code = r.Code,
                Date = r.Date,
                VehicleId = r.VehicleId,
                VehicleCode = r.Vehicle != null ? r.Vehicle.Code : null,
                DriverEmployeeId = r.DriverEmployeeId,
                DriverName = r.DriverEmployee != null
                    ? r.DriverEmployee.LastName + " " + r.DriverEmployee.FirstName
                    : null,
                Status = r.Status,
                Notes = r.Notes,
                StopCount = r.Stops.Count
            })
            .ToListAsync(ct);
    }
}

public record GetDeliveryRouteByIdQuery(Guid Id) : IRequest<RouteDto?>;

public class GetDeliveryRouteByIdQueryHandler : IRequestHandler<GetDeliveryRouteByIdQuery, RouteDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDeliveryRouteByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<RouteDto?> Handle(GetDeliveryRouteByIdQuery request, CancellationToken ct)
    {
        return await _context.DeliveryRoutes
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new RouteDto
            {
                Id = r.Id,
                Code = r.Code,
                Date = r.Date,
                VehicleId = r.VehicleId,
                VehicleCode = r.Vehicle != null ? r.Vehicle.Code : null,
                DriverEmployeeId = r.DriverEmployeeId,
                DriverName = r.DriverEmployee != null
                    ? r.DriverEmployee.LastName + " " + r.DriverEmployee.FirstName
                    : null,
                Status = r.Status,
                Notes = r.Notes,
                StopCount = r.Stops.Count
            })
            .FirstOrDefaultAsync(ct);
    }
}

public record GetDeliveryRouteDetailQuery(Guid Id) : IRequest<RouteDetailDto?>;

public class GetDeliveryRouteDetailQueryHandler : IRequestHandler<GetDeliveryRouteDetailQuery, RouteDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDeliveryRouteDetailQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<RouteDetailDto?> Handle(GetDeliveryRouteDetailQuery request, CancellationToken ct)
    {
        return await _context.DeliveryRoutes
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new RouteDetailDto
            {
                Id = r.Id,
                Code = r.Code,
                Date = r.Date,
                VehicleId = r.VehicleId,
                VehicleCode = r.Vehicle != null ? r.Vehicle.Code : null,
                DriverEmployeeId = r.DriverEmployeeId,
                DriverName = r.DriverEmployee != null
                    ? r.DriverEmployee.LastName + " " + r.DriverEmployee.FirstName
                    : null,
                Status = r.Status,
                Notes = r.Notes,
                StopCount = r.Stops.Count,
                Stops = r.Stops
                    .OrderBy(s => s.SequenceNumber)
                    .Select(s => new StopDto
                    {
                        Id = s.Id,
                        RouteId = s.RouteId,
                        SequenceNumber = s.SequenceNumber,
                        CustomerId = s.CustomerId,
                        CustomerName = s.Customer != null ? s.Customer.Name : null,
                        Address = s.Address,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        PlannedArrivalUtc = s.PlannedArrivalUtc,
                        PlannedDepartureUtc = s.PlannedDepartureUtc,
                        ActualArrivalUtc = s.ActualArrivalUtc,
                        ActualDepartureUtc = s.ActualDepartureUtc,
                        Status = s.Status,
                        HasRegulatedGoods = s.HasRegulatedGoods,
                        Notes = s.Notes,
                        ShipmentCount = s.Shipments.Count,
                        Shipments = s.Shipments
                            .OrderBy(sh => sh.SalesOrder != null ? sh.SalesOrder.Number : "")
                            .Select(sh => new ShipmentDto
                            {
                                Id = sh.Id,
                                SalesOrderNumber = sh.SalesOrder != null ? sh.SalesOrder.Number : "",
                                CustomerName = sh.SalesOrder != null && sh.SalesOrder.Customer != null ? sh.SalesOrder.Customer.Name : null,
                                Status = sh.Status
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);
    }
}

/// <summary>Returns routes assigned to the currently authenticated driver.</summary>
public record GetMyRoutesQuery : IRequest<IReadOnlyList<RouteDto>>;

public class GetMyRoutesQueryHandler : IRequestHandler<GetMyRoutesQuery, IReadOnlyList<RouteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyRoutesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RouteDto>> Handle(GetMyRoutesQuery request, CancellationToken ct)
    {
        var employeeId = _currentUser.EmployeeId;
        if (!employeeId.HasValue) return Array.Empty<RouteDto>();

        return await _context.DeliveryRoutes
            .AsNoTracking()
            .Where(r => r.DriverEmployeeId == employeeId.Value)
            .Where(r => r.Status == Domain.Enums.RouteStatus.Planned || r.Status == Domain.Enums.RouteStatus.InProgress || r.Status == Domain.Enums.RouteStatus.Completed)
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.Code)
            .Select(r => new RouteDto
            {
                Id = r.Id,
                Code = r.Code,
                Date = r.Date,
                VehicleId = r.VehicleId,
                VehicleCode = r.Vehicle != null ? r.Vehicle.Code : null,
                DriverEmployeeId = r.DriverEmployeeId,
                DriverName = r.DriverEmployee != null
                    ? r.DriverEmployee.LastName + " " + r.DriverEmployee.FirstName
                    : null,
                Status = r.Status,
                Notes = r.Notes,
                StopCount = r.Stops.Count
            })
            .ToListAsync(ct);
    }
}

/// <summary>Returns full route detail for a driver's own route.</summary>
public record GetMyRouteDetailQuery(Guid Id) : IRequest<RouteDetailDto?>;

public class GetMyRouteDetailQueryHandler : IRequestHandler<GetMyRouteDetailQuery, RouteDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyRouteDetailQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<RouteDetailDto?> Handle(GetMyRouteDetailQuery request, CancellationToken ct)
    {
        var employeeId = _currentUser.EmployeeId;
        if (!employeeId.HasValue) return null;

        return await _context.DeliveryRoutes
            .AsNoTracking()
            .Where(r => r.Id == request.Id && r.DriverEmployeeId == employeeId.Value)
            .Select(r => new RouteDetailDto
            {
                Id = r.Id,
                Code = r.Code,
                Date = r.Date,
                VehicleId = r.VehicleId,
                VehicleCode = r.Vehicle != null ? r.Vehicle.Code : null,
                DriverEmployeeId = r.DriverEmployeeId,
                DriverName = r.DriverEmployee != null
                    ? r.DriverEmployee.LastName + " " + r.DriverEmployee.FirstName
                    : null,
                Status = r.Status,
                Notes = r.Notes,
                StopCount = r.Stops.Count,
                Stops = r.Stops.OrderBy(s => s.SequenceNumber).Select(s => new StopDto
                {
                    Id = s.Id, RouteId = s.RouteId,
                    SequenceNumber = s.SequenceNumber,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer != null ? s.Customer.Name : null,
                    Address = s.Address,
                    Latitude = s.Latitude, Longitude = s.Longitude,
                    PlannedArrivalUtc = s.PlannedArrivalUtc,
                    PlannedDepartureUtc = s.PlannedDepartureUtc,
                    ActualArrivalUtc = s.ActualArrivalUtc,
                    ActualDepartureUtc = s.ActualDepartureUtc,
                    Status = s.Status,
                    HasRegulatedGoods = s.HasRegulatedGoods,
                    Notes = s.Notes,
                    ShipmentCount = s.Shipments.Count,
                    Shipments = s.Shipments.OrderBy(sh => sh.SalesOrder != null ? sh.SalesOrder.Number : "").Select(sh => new ShipmentDto
                    {
                        Id = sh.Id,
                        SalesOrderNumber = sh.SalesOrder != null ? sh.SalesOrder.Number : "",
                        CustomerName = sh.SalesOrder != null && sh.SalesOrder.Customer != null ? sh.SalesOrder.Customer.Name : null,
                        Status = sh.Status
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }
}
