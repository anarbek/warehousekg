using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(32).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(512);
        builder.HasIndex(p => new { p.TenantId, p.Code }).IsUnique();
    }
}
