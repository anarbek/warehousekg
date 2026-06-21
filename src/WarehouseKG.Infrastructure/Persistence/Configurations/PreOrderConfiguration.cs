using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class PreOrderConfiguration : IEntityTypeConfiguration<PreOrder>
{
    public void Configure(EntityTypeBuilder<PreOrder> builder)
    {
        builder.ToTable("pre_orders");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Number).HasMaxLength(64).IsRequired();
        builder.Property(p => p.PaymentType).HasMaxLength(64).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Notes).HasMaxLength(1024);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.TotalAmount).HasColumnType("numeric(18,2)");

        builder.HasIndex(p => new { p.TenantId, p.Number }).IsUnique();
        builder.HasIndex(p => new { p.TenantId, p.CustomerId });
        builder.HasIndex(p => new { p.TenantId, p.PresellerId });
        builder.HasIndex(p => new { p.TenantId, p.Status });

        builder.HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Preseller)
            .WithMany()
            .HasForeignKey(p => p.PresellerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Warehouse)
            .WithMany()
            .HasForeignKey(p => p.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ConvertedSalesOrder)
            .WithMany()
            .HasForeignKey(p => p.ConvertedSalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Lines)
            .WithOne(l => l.PreOrder!)
            .HasForeignKey(l => l.PreOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PreOrderLineConfiguration : IEntityTypeConfiguration<PreOrderLine>
{
    public void Configure(EntityTypeBuilder<PreOrderLine> builder)
    {
        builder.ToTable("pre_order_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Quantity).HasColumnType("numeric(18,3)");
        builder.Property(l => l.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(l => l.WarehouseStockSnapshot).HasColumnType("numeric(18,3)");
        builder.Property(l => l.StockDifference).HasColumnType("numeric(18,3)");
        builder.Property(l => l.DiscountPercent).HasColumnType("numeric(5,2)");
        builder.Property(l => l.LineTotal).HasColumnType("numeric(18,2)");

        builder.HasOne(l => l.InventoryItem)
            .WithMany()
            .HasForeignKey(l => l.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
