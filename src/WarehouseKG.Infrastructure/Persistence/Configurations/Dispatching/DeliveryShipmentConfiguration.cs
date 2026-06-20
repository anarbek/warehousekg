using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Dispatching;

public class DeliveryShipmentConfiguration : IEntityTypeConfiguration<DeliveryShipment>
{
    public void Configure(EntityTypeBuilder<DeliveryShipment> builder)
    {
        builder.ToTable("delivery_shipments");
        builder.HasKey(sh => sh.Id);
        builder.Property(sh => sh.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(sh => sh.SalesOrderId).IsUnique();

        builder.HasOne(sh => sh.DeliveryStop)
            .WithMany(s => s.Shipments)
            .HasForeignKey(sh => sh.DeliveryStopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sh => sh.SalesOrder)
            .WithMany()
            .HasForeignKey(sh => sh.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
