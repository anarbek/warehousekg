using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("units_of_measure");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Code).HasMaxLength(16).IsRequired();
        builder.Property(u => u.Name).HasMaxLength(128).IsRequired();
        builder.Property(u => u.Description).HasMaxLength(512);

        builder.HasIndex(u => new { u.TenantId, u.Code }).IsUnique();
    }
}
