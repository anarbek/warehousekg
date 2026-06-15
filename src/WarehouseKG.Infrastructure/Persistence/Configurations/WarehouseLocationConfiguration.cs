using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class WarehouseLocationConfiguration : IEntityTypeConfiguration<WarehouseLocation>
{
    public void Configure(EntityTypeBuilder<WarehouseLocation> builder)
    {
        builder.ToTable("warehouse_locations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Code).HasMaxLength(64).IsRequired();
        builder.Property(l => l.Zone).HasMaxLength(64);
        builder.Property(l => l.Aisle).HasMaxLength(64);
        builder.Property(l => l.Bin).HasMaxLength(64);

        builder.HasIndex(l => new { l.TenantId, l.WarehouseId, l.Code }).IsUnique();
    }
}
