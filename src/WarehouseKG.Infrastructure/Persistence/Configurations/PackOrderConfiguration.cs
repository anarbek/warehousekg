using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class PackOrderConfiguration : IEntityTypeConfiguration<PackOrder>
{
    public void Configure(EntityTypeBuilder<PackOrder> builder)
    {
        builder.ToTable("pack_orders");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Number).HasMaxLength(64).IsRequired();
        builder.Property(p => p.Notes).HasMaxLength(1024);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(p => new { p.TenantId, p.Number }).IsUnique();

        builder.HasOne(p => p.Warehouse)
            .WithMany()
            .HasForeignKey(p => p.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PickOrder)
            .WithMany()
            .HasForeignKey(p => p.PickOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Lines)
            .WithOne(l => l.PackOrder!)
            .HasForeignKey(l => l.PackOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PackOrderLineConfiguration : IEntityTypeConfiguration<PackOrderLine>
{
    public void Configure(EntityTypeBuilder<PackOrderLine> builder)
    {
        builder.ToTable("pack_order_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Quantity).HasColumnType("numeric(18,3)");
        builder.Property(l => l.PackageLabel).HasMaxLength(128);

        builder.HasOne(l => l.InventoryItem)
            .WithMany()
            .HasForeignKey(l => l.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
