using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Code).HasMaxLength(32).IsRequired();
        builder.Property(v => v.LicensePlate).HasMaxLength(32).IsRequired();
        builder.Property(v => v.VIN).HasMaxLength(64);
        builder.Property(v => v.Brand).HasMaxLength(128).IsRequired();
        builder.Property(v => v.Model).HasMaxLength(128);
        builder.Property(v => v.OwnershipType).HasConversion<string>().HasMaxLength(32);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(v => v.FuelType).HasConversion<string>().HasMaxLength(32);
        builder.Property(v => v.FuelConsumptionRate).HasColumnType("decimal(8,2)");
        builder.Property(v => v.MaxCapacityKg).HasColumnType("decimal(18,3)");
        builder.Property(v => v.MaxCapacityM3).HasColumnType("decimal(18,3)");
        builder.Property(v => v.CurrentMileageKm).HasColumnType("decimal(18,1)");
        builder.Property(v => v.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(v => v.InsurancePolicyNumber).HasMaxLength(128);
        builder.Property(v => v.InsuranceProvider).HasMaxLength(256);
        builder.Property(v => v.NextMaintenanceMileageKm).HasColumnType("decimal(18,1)");
        builder.Property(v => v.Notes).HasMaxLength(1024);

        builder.HasIndex(v => new { v.TenantId, v.Code }).IsUnique();
        builder.HasIndex(v => new { v.TenantId, v.LicensePlate }).IsUnique();

        builder.HasOne(v => v.VehicleType)
            .WithMany()
            .HasForeignKey(v => v.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
