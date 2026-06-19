using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Vehicles.Vehicles.Commands;

public record CreateVehicleCommand(
    string Code, string LicensePlate, string? VIN, string Brand, string? Model,
    int? ManufactureYear, Guid? VehicleTypeId,
    VehicleOwnershipType OwnershipType, VehicleStatus Status, FuelType FuelType,
    decimal? FuelConsumptionRate, decimal? MaxCapacityKg, decimal? MaxCapacityM3,
    decimal CurrentMileageKm, DateTime? PurchaseDate, decimal? PurchasePrice,
    string? InsurancePolicyNumber, string? InsuranceProvider, DateTime? InsuranceExpiryDate,
    DateTime? TechInspectionExpiryDate,
    decimal? NextMaintenanceMileageKm, DateTime? NextMaintenanceDate,
    bool HasGpsTracker, string? Notes) : IRequest<Guid>;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateVehicleCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Guid> Handle(CreateVehicleCommand r, CancellationToken ct)
    {
        var v = new Vehicle
        {
            Id = Guid.NewGuid(), Code = r.Code, LicensePlate = r.LicensePlate,
            VIN = r.VIN, Brand = r.Brand, Model = r.Model,
            ManufactureYear = r.ManufactureYear, VehicleTypeId = r.VehicleTypeId,
            OwnershipType = r.OwnershipType, Status = r.Status, FuelType = r.FuelType,
            FuelConsumptionRate = r.FuelConsumptionRate,
            MaxCapacityKg = r.MaxCapacityKg, MaxCapacityM3 = r.MaxCapacityM3,
            CurrentMileageKm = r.CurrentMileageKm,
            PurchaseDate = r.PurchaseDate, PurchasePrice = r.PurchasePrice,
            InsurancePolicyNumber = r.InsurancePolicyNumber,
            InsuranceProvider = r.InsuranceProvider,
            InsuranceExpiryDate = r.InsuranceExpiryDate,
            TechInspectionExpiryDate = r.TechInspectionExpiryDate,
            NextMaintenanceMileageKm = r.NextMaintenanceMileageKm,
            NextMaintenanceDate = r.NextMaintenanceDate,
            HasGpsTracker = r.HasGpsTracker, Notes = r.Notes
        };
        _context.Vehicles.Add(v);
        await _context.SaveChangesAsync(ct);
        return v.Id;
    }
}
