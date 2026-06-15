using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasMaxLength(32).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
        builder.Property(c => c.ContactName).HasMaxLength(256);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Phone).HasMaxLength(64);
        builder.Property(c => c.Address).HasMaxLength(512);
        builder.Property(c => c.TaxId).HasMaxLength(64);

        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
    }
}
