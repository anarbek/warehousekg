using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleFuelRecordConfiguration : IEntityTypeConfiguration<VehicleFuelRecord>
{
    public void Configure(EntityTypeBuilder<VehicleFuelRecord> builder)
    {
        builder.ToTable("vehicle_fuel_records");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Liters).HasColumnType("decimal(10,2)");
        builder.Property(r => r.Cost).HasColumnType("decimal(18,2)");
        builder.Property(r => r.MileageKm).HasColumnType("decimal(18,1)");
        builder.Property(r => r.FuelType).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.Station).HasMaxLength(256);
        builder.Property(r => r.Notes).HasMaxLength(1024);

        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.FuelRecords)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
