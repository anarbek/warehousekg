using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class StockAuditConfiguration : IEntityTypeConfiguration<StockAudit>
{
    public void Configure(EntityTypeBuilder<StockAudit> builder)
    {
        builder.ToTable("stock_audits");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Number).HasMaxLength(64).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(1024);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(a => new { a.TenantId, a.Number }).IsUnique();

        builder.HasOne(a => a.Warehouse)
            .WithMany()
            .HasForeignKey(a => a.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Lines)
            .WithOne(l => l.StockAudit!)
            .HasForeignKey(l => l.StockAuditId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockAuditLineConfiguration : IEntityTypeConfiguration<StockAuditLine>
{
    public void Configure(EntityTypeBuilder<StockAuditLine> builder)
    {
        builder.ToTable("stock_audit_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.SystemQuantity).HasColumnType("numeric(18,3)");
        builder.Property(l => l.CountedQuantity).HasColumnType("numeric(18,3)");

        builder.HasOne(l => l.InventoryItem)
            .WithMany()
            .HasForeignKey(l => l.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
