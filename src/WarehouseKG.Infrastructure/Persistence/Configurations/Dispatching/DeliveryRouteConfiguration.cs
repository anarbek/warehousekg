using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Dispatching;

public class DeliveryRouteConfiguration : IEntityTypeConfiguration<DeliveryRoute>
{
    public void Configure(EntityTypeBuilder<DeliveryRoute> builder)
    {
        builder.ToTable("delivery_routes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Code).HasMaxLength(32).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.Notes).HasMaxLength(1024);

        builder.HasIndex(r => new { r.TenantId, r.Code }).IsUnique();
        builder.HasIndex(r => new { r.TenantId, r.Date });

        builder.HasOne(r => r.Vehicle)
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.DriverEmployee)
            .WithMany()
            .HasForeignKey(r => r.DriverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
