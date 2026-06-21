using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Vehicles.Vehicles.Dtos;

namespace WarehouseKG.Application.Features.Vehicles.Vehicles.Queries;

public record GetVehiclesQuery : IRequest<IReadOnlyList<VehicleDto>>;

public class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, IReadOnlyList<VehicleDto>>
{
    private readonly IApplicationDbContext _context;
    public GetVehiclesQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<IReadOnlyList<VehicleDto>> Handle(GetVehiclesQuery r, CancellationToken ct)
        => await _context.Vehicles.AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new VehicleDto
            {
                Id = x.Id, Code = x.Code, LicensePlate = x.LicensePlate,
                VIN = x.VIN, Brand = x.Brand, Model = x.Model,
                ManufactureYear = x.ManufactureYear,
                VehicleTypeId = x.VehicleTypeId,
                VehicleTypeName = x.VehicleType != null ? x.VehicleType.Name : null,
                OwnershipType = x.OwnershipType.ToString(),
                Status = x.Status.ToString(), FuelType = x.FuelType.ToString(),
                FuelConsumptionRate = x.FuelConsumptionRate,
                MaxCapacityKg = x.MaxCapacityKg, MaxCapacityM3 = x.MaxCapacityM3,
                CurrentMileageKm = x.CurrentMileageKm,
                PurchaseDate = x.PurchaseDate, PurchasePrice = x.PurchasePrice,
                InsurancePolicyNumber = x.InsurancePolicyNumber,
                InsuranceProvider = x.InsuranceProvider,
                InsuranceExpiryDate = x.InsuranceExpiryDate,
                TechInspectionExpiryDate = x.TechInspectionExpiryDate,
                NextMaintenanceMileageKm = x.NextMaintenanceMileageKm,
                NextMaintenanceDate = x.NextMaintenanceDate,
                HasGpsTracker = x.HasGpsTracker, Notes = x.Notes,
                AssignedDriverName = x.DriverAssignments
                    .Where(a => a.AssignedToUtc == null)
                    .Select(a => a.Employee != null ? a.Employee.LastName + " " + a.Employee.FirstName : null)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);
}

public record GetVehicleByIdQuery(Guid Id) : IRequest<VehicleDto?>;

public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDto?>
{
    private readonly IApplicationDbContext _context;
    public GetVehicleByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<VehicleDto?> Handle(GetVehicleByIdQuery r, CancellationToken ct)
        => await _context.Vehicles.AsNoTracking().Where(x => x.Id == r.Id)
            .Select(x => new VehicleDto
            {
                Id = x.Id, Code = x.Code, LicensePlate = x.LicensePlate,
                VIN = x.VIN, Brand = x.Brand, Model = x.Model,
                ManufactureYear = x.ManufactureYear,
                VehicleTypeId = x.VehicleTypeId,
                VehicleTypeName = x.VehicleType != null ? x.VehicleType.Name : null,
                OwnershipType = x.OwnershipType.ToString(),
                Status = x.Status.ToString(), FuelType = x.FuelType.ToString(),
                FuelConsumptionRate = x.FuelConsumptionRate,
                MaxCapacityKg = x.MaxCapacityKg, MaxCapacityM3 = x.MaxCapacityM3,
                CurrentMileageKm = x.CurrentMileageKm,
                PurchaseDate = x.PurchaseDate, PurchasePrice = x.PurchasePrice,
                InsurancePolicyNumber = x.InsurancePolicyNumber,
                InsuranceProvider = x.InsuranceProvider,
                InsuranceExpiryDate = x.InsuranceExpiryDate,
                TechInspectionExpiryDate = x.TechInspectionExpiryDate,
                NextMaintenanceMileageKm = x.NextMaintenanceMileageKm,
                NextMaintenanceDate = x.NextMaintenanceDate,
                HasGpsTracker = x.HasGpsTracker, Notes = x.Notes,
                AssignedDriverName = x.DriverAssignments
                    .Where(a => a.AssignedToUtc == null)
                    .Select(a => a.Employee != null ? a.Employee.LastName + " " + a.Employee.FirstName : null)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);
}

public record GetVehicleDetailQuery(Guid Id) : IRequest<VehicleDetailDto?>;

public class GetVehicleDetailQueryHandler : IRequestHandler<GetVehicleDetailQuery, VehicleDetailDto?>
{
    private readonly IApplicationDbContext _context;
    public GetVehicleDetailQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<VehicleDetailDto?> Handle(GetVehicleDetailQuery r, CancellationToken ct)
    {
        var v = await _context.Vehicles.AsNoTracking().Where(x => x.Id == r.Id)
            .Select(x => new VehicleDetailDto
            {
                Id = x.Id, Code = x.Code, LicensePlate = x.LicensePlate,
                VIN = x.VIN, Brand = x.Brand, Model = x.Model,
                ManufactureYear = x.ManufactureYear,
                VehicleTypeId = x.VehicleTypeId,
                VehicleTypeName = x.VehicleType != null ? x.VehicleType.Name : null,
                OwnershipType = x.OwnershipType.ToString(),
                Status = x.Status.ToString(), FuelType = x.FuelType.ToString(),
                FuelConsumptionRate = x.FuelConsumptionRate,
                MaxCapacityKg = x.MaxCapacityKg, MaxCapacityM3 = x.MaxCapacityM3,
                CurrentMileageKm = x.CurrentMileageKm,
                PurchaseDate = x.PurchaseDate, PurchasePrice = x.PurchasePrice,
                InsurancePolicyNumber = x.InsurancePolicyNumber,
                InsuranceProvider = x.InsuranceProvider,
                InsuranceExpiryDate = x.InsuranceExpiryDate,
                TechInspectionExpiryDate = x.TechInspectionExpiryDate,
                NextMaintenanceMileageKm = x.NextMaintenanceMileageKm,
                NextMaintenanceDate = x.NextMaintenanceDate,
                HasGpsTracker = x.HasGpsTracker, Notes = x.Notes,
                AssignedDriverName = x.DriverAssignments
                    .Where(a => a.AssignedToUtc == null)
                    .Select(a => a.Employee != null ? a.Employee.LastName + " " + a.Employee.FirstName : null)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);
        if (v is null) return null;

        v.DriverAssignments = await _context.VehicleDriverAssignments.AsNoTracking()
            .Where(a => a.VehicleId == r.Id).OrderByDescending(a => a.AssignedFromUtc)
            .Select(a => new DriverAssignmentDto
            {
                Id = a.Id, EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee != null ? a.Employee.LastName + " " + a.Employee.FirstName : null,
                AssignedFromUtc = a.AssignedFromUtc, AssignedToUtc = a.AssignedToUtc,
                IsPrimary = a.IsPrimary
            })
            .ToListAsync(ct);

        v.MaintenanceRecords = await _context.VehicleMaintenanceRecords.AsNoTracking()
            .Where(m => m.VehicleId == r.Id).OrderByDescending(m => m.Date)
            .Select(m => new MaintenanceRecordDto
            {
                Id = m.Id,
                VehicleId = m.VehicleId,
                MaintenanceType = m.MaintenanceType.ToString(),
                Date = m.Date, MileageKm = m.MileageKm, Cost = m.Cost,
                Description = m.Description, ServiceProvider = m.ServiceProvider,
                Notes = m.Notes, NextDueMileageKm = m.NextDueMileageKm,
                NextDueDate = m.NextDueDate
            })
            .ToListAsync(ct);

        v.InsuranceRecords = await _context.VehicleInsuranceRecords.AsNoTracking()
            .Where(i => i.VehicleId == r.Id).OrderByDescending(i => i.StartDate)
            .Select(i => new InsuranceRecordDto
            {
                Id = i.Id,
                VehicleId = i.VehicleId,
                PolicyNumber = i.PolicyNumber, Provider = i.Provider,
                CoverageType = i.CoverageType, StartDate = i.StartDate,
                EndDate = i.EndDate, PremiumAmount = i.PremiumAmount,
                Description = i.Description
            })
            .ToListAsync(ct);

        v.InspectionRecords = await _context.VehicleInspectionRecords.AsNoTracking()
            .Where(i => i.VehicleId == r.Id).OrderByDescending(i => i.InspectionDate)
            .Select(i => new InspectionRecordDto
            {
                Id = i.Id,
                VehicleId = i.VehicleId,
                InspectionDate = i.InspectionDate, ExpiryDate = i.ExpiryDate,
                Result = i.Result.ToString(), Inspector = i.Inspector, Notes = i.Notes
            })
            .ToListAsync(ct);

        return v;
    }
}
