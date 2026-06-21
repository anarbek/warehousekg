using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).HasMaxLength(256).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(64).IsRequired();
        builder.Property(t => t.ContactEmail).HasMaxLength(256);
        builder.Property(t => t.ContactPhone).HasMaxLength(32);
        builder.Property(t => t.DefaultCurrency).HasMaxLength(3).IsRequired();
        builder.Property(t => t.EnabledModules).HasMaxLength(1024);

        builder.HasIndex(t => new { t.TenantId, t.Slug }).IsUnique();
        builder.HasIndex(t => new { t.TenantId, t.Name });
    }
}
