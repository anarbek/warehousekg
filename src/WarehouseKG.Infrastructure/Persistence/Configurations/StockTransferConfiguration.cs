using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("stock_transfers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Number).HasMaxLength(64).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(1024);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(t => new { t.TenantId, t.Number }).IsUnique();

        builder.HasOne(t => t.SourceWarehouse)
            .WithMany()
            .HasForeignKey(t => t.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(t => t.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Employee)
            .WithMany()
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Lines)
            .WithOne(l => l.StockTransfer!)
            .HasForeignKey(l => l.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockTransferLineConfiguration : IEntityTypeConfiguration<StockTransferLine>
{
    public void Configure(EntityTypeBuilder<StockTransferLine> builder)
    {
        builder.ToTable("stock_transfer_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Quantity).HasColumnType("numeric(18,3)");

        builder.HasOne(l => l.InventoryItem)
            .WithMany()
            .HasForeignKey(l => l.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
