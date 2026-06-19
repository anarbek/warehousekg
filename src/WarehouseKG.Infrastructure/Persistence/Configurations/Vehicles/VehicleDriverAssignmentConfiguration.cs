using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleDriverAssignmentConfiguration : IEntityTypeConfiguration<VehicleDriverAssignment>
{
    public void Configure(EntityTypeBuilder<VehicleDriverAssignment> builder)
    {
        builder.ToTable("vehicle_driver_assignments");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.TenantId, a.VehicleId, a.EmployeeId, a.AssignedFromUtc }).IsUnique();

        builder.HasOne(a => a.Vehicle)
            .WithMany(v => v.DriverAssignments)
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
