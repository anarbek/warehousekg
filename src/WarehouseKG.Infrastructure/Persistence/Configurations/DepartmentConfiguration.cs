using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Code).HasMaxLength(32).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(256).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(512);
        builder.HasIndex(d => new { d.TenantId, d.Code }).IsUnique();
    }
}
