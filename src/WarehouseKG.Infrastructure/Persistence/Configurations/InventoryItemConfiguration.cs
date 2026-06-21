using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku).HasMaxLength(64).IsRequired();
        builder.Property(i => i.Name).HasMaxLength(256).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(1024);
        builder.Property(i => i.Barcode).HasMaxLength(128);
        builder.Property(i => i.QuantityOnHand).HasColumnType("numeric(18,3)");
        builder.Property(i => i.ReorderLevel).HasColumnType("numeric(18,3)");
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(18,2)");

        builder.HasIndex(i => new { i.TenantId, i.Sku }).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.Barcode });

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.UnitOfMeasure)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
