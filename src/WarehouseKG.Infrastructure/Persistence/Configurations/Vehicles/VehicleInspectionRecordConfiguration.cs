using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleInspectionRecordConfiguration : IEntityTypeConfiguration<VehicleInspectionRecord>
{
    public void Configure(EntityTypeBuilder<VehicleInspectionRecord> builder)
    {
        builder.ToTable("vehicle_inspection_records");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Result).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.Inspector).HasMaxLength(256);
        builder.Property(r => r.Notes).HasMaxLength(1024);

        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.InspectionRecords)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
