using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class StockReceiptConfiguration : IEntityTypeConfiguration<StockReceipt>
{
    public void Configure(EntityTypeBuilder<StockReceipt> builder)
    {
        builder.ToTable("stock_receipts");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Number).HasMaxLength(64).IsRequired();
        builder.Property(r => r.SupplierReference).HasMaxLength(128);
        builder.Property(r => r.Notes).HasMaxLength(1024);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.TransactionDate).IsRequired();

        builder.HasIndex(r => new { r.TenantId, r.Number }).IsUnique();

        builder.HasOne(r => r.Warehouse)
            .WithMany()
            .HasForeignKey(r => r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Lines)
            .WithOne(l => l.StockReceipt!)
            .HasForeignKey(l => l.StockReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockReceiptLineConfiguration : IEntityTypeConfiguration<StockReceiptLine>
{
    public void Configure(EntityTypeBuilder<StockReceiptLine> builder)
    {
        builder.ToTable("stock_receipt_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Quantity).HasColumnType("numeric(18,3)");

        builder.HasOne(l => l.InventoryItem)
            .WithMany()
            .HasForeignKey(l => l.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.WarehouseLocation)
            .WithMany()
            .HasForeignKey(l => l.WarehouseLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
