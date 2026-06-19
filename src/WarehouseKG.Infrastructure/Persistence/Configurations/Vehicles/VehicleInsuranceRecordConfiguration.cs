using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleInsuranceRecordConfiguration : IEntityTypeConfiguration<VehicleInsuranceRecord>
{
    public void Configure(EntityTypeBuilder<VehicleInsuranceRecord> builder)
    {
        builder.ToTable("vehicle_insurance_records");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.PolicyNumber).HasMaxLength(128).IsRequired();
        builder.Property(r => r.Provider).HasMaxLength(256).IsRequired();
        builder.Property(r => r.CoverageType).HasMaxLength(256);
        builder.Property(r => r.PremiumAmount).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Description).HasMaxLength(1024);

        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.InsuranceRecords)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
