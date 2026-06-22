using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Number).HasMaxLength(64).IsRequired();
        builder.Property(i => i.Type).HasConversion<string>().HasMaxLength(16);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(i => i.Currency).HasMaxLength(3).IsRequired();
        builder.Property(i => i.PaymentType).HasMaxLength(32);
        builder.Property(i => i.Notes).HasMaxLength(1024);
        builder.Property(i => i.ExternalReference).HasMaxLength(128);
        builder.Property(i => i.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("numeric(18,2)");
        builder.Property(i => i.ExchangeRate).HasColumnType("numeric(18,6)");
        builder.Property(i => i.ReportLayoutId).HasMaxLength(64);
        builder.Property(i => i.PrintedBy).HasMaxLength(256);
        builder.Property(i => i.SignedByName).HasMaxLength(256);

        builder.HasIndex(i => new { i.TenantId, i.Number }).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.CustomerId });
        builder.HasIndex(i => new { i.TenantId, i.SalesOrderId });
        builder.HasIndex(i => new { i.TenantId, i.Status });

        builder.HasOne(i => i.SalesOrder)
            .WithMany()
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PurchaseOrder)
            .WithMany()
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Supplier)
            .WithMany()
            .HasForeignKey(i => i.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Warehouse)
            .WithMany()
            .HasForeignKey(i => i.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Lines)
            .WithOne(l => l.Invoice!)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Quantity).HasColumnType("numeric(18,3)");
        builder.Property(l => l.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(l => l.LineTotal).HasColumnType("numeric(18,2)");
        builder.Property(l => l.TaxRate).HasColumnType("numeric(6,4)");
        builder.Property(l => l.TaxAmount).HasColumnType("numeric(18,2)");
        builder.Property(l => l.Notes).HasMaxLength(512);

        builder.HasOne(l => l.InventoryItem)
            .WithMany()
            .HasForeignKey(l => l.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
