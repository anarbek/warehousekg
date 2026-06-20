using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Dispatching;

public class GeofenceConfiguration : IEntityTypeConfiguration<Geofence>
{
    public void Configure(EntityTypeBuilder<Geofence> builder)
    {
        builder.ToTable("geofences");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Code).HasMaxLength(32).IsRequired();
        builder.Property(g => g.Name).HasMaxLength(256).IsRequired();
        builder.Property(g => g.Type).HasConversion<string>().HasMaxLength(32);

        // Store polygon vertices as JSONB array
        builder.OwnsMany(g => g.Vertices, v =>
        {
            v.ToJson();
        });

        builder.HasIndex(g => new { g.TenantId, g.Code }).IsUnique();
    }
}
