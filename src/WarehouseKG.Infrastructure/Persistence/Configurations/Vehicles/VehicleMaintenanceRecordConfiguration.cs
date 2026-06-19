using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleMaintenanceRecordConfiguration : IEntityTypeConfiguration<VehicleMaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<VehicleMaintenanceRecord> builder)
    {
        builder.ToTable("vehicle_maintenance_records");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.MaintenanceType).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.MileageKm).HasColumnType("decimal(18,1)");
        builder.Property(r => r.Cost).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Description).HasMaxLength(1024);
        builder.Property(r => r.ServiceProvider).HasMaxLength(256);
        builder.Property(r => r.Notes).HasMaxLength(1024);
        builder.Property(r => r.NextDueMileageKm).HasColumnType("decimal(18,1)");

        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.MaintenanceRecords)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
