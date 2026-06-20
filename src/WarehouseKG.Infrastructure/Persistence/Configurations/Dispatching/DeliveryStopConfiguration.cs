using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Dispatching;

public class DeliveryStopConfiguration : IEntityTypeConfiguration<DeliveryStop>
{
    public void Configure(EntityTypeBuilder<DeliveryStop> builder)
    {
        builder.ToTable("delivery_stops");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Address).HasMaxLength(512).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(s => s.Notes).HasMaxLength(1024);

        builder.HasIndex(s => new { s.RouteId, s.SequenceNumber }).IsUnique();

        builder.HasOne(s => s.Route)
            .WithMany(r => r.Stops)
            .HasForeignKey(s => s.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
