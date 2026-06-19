using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("shifts");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).HasMaxLength(32).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(128).IsRequired();
        builder.HasIndex(s => new { s.TenantId, s.Code }).IsUnique();
    }
}
