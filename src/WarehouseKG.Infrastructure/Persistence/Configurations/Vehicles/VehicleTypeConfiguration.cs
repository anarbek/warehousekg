using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
{
    public void Configure(EntityTypeBuilder<VehicleType> builder)
    {
        builder.ToTable("vehicle_types");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).HasMaxLength(32).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(256).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(512);
        builder.Property(t => t.DefaultCapacityKg).HasColumnType("decimal(18,3)");
        builder.Property(t => t.DefaultCapacityM3).HasColumnType("decimal(18,3)");
        builder.HasIndex(t => new { t.TenantId, t.Code }).IsUnique();
    }
}
