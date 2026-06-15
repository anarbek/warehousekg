using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code).HasMaxLength(32).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(256).IsRequired();
        builder.Property(s => s.ContactName).HasMaxLength(256);
        builder.Property(s => s.Email).HasMaxLength(256);
        builder.Property(s => s.Phone).HasMaxLength(64);
        builder.Property(s => s.Address).HasMaxLength(512);
        builder.Property(s => s.TaxId).HasMaxLength(64);

        builder.HasIndex(s => new { s.TenantId, s.Code }).IsUnique();
    }
}
