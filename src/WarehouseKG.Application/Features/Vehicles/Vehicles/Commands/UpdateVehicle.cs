using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Vehicles.Vehicles.Commands;

public record UpdateVehicleCommand(
    Guid Id, string Code, string LicensePlate, string? VIN, string Brand, string? Model,
    int? ManufactureYear, Guid? VehicleTypeId,
    VehicleOwnershipType OwnershipType, VehicleStatus Status, FuelType FuelType,
    decimal? FuelConsumptionRate, decimal? MaxCapacityKg, decimal? MaxCapacityM3,
    decimal CurrentMileageKm, DateTime? PurchaseDate, decimal? PurchasePrice,
    string? InsurancePolicyNumber, string? InsuranceProvider, DateTime? InsuranceExpiryDate,
    DateTime? TechInspectionExpiryDate,
    decimal? NextMaintenanceMileageKm, DateTime? NextMaintenanceDate,
    bool HasGpsTracker, string? Notes) : IRequest<bool>;

public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateVehicleCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<bool> Handle(UpdateVehicleCommand r, CancellationToken ct)
    {
        var v = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
        if (v is null) return false;
        v.Code = r.Code; v.LicensePlate = r.LicensePlate; v.VIN = r.VIN;
        v.Brand = r.Brand; v.Model = r.Model; v.ManufactureYear = r.ManufactureYear;
        v.VehicleTypeId = r.VehicleTypeId;
        v.OwnershipType = r.OwnershipType; v.Status = r.Status; v.FuelType = r.FuelType;
        v.FuelConsumptionRate = r.FuelConsumptionRate;
        v.MaxCapacityKg = r.MaxCapacityKg; v.MaxCapacityM3 = r.MaxCapacityM3;
        v.CurrentMileageKm = r.CurrentMileageKm;
        v.PurchaseDate = r.PurchaseDate; v.PurchasePrice = r.PurchasePrice;
        v.InsurancePolicyNumber = r.InsurancePolicyNumber;
        v.InsuranceProvider = r.InsuranceProvider;
        v.InsuranceExpiryDate = r.InsuranceExpiryDate;
        v.TechInspectionExpiryDate = r.TechInspectionExpiryDate;
        v.NextMaintenanceMileageKm = r.NextMaintenanceMileageKm;
        v.NextMaintenanceDate = r.NextMaintenanceDate;
        v.HasGpsTracker = r.HasGpsTracker; v.Notes = r.Notes;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
