using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class EmployeeWarehouseAssignmentConfiguration : IEntityTypeConfiguration<EmployeeWarehouseAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeWarehouseAssignment> builder)
    {
        builder.ToTable("employee_warehouse_assignments");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.TenantId, a.EmployeeId, a.WarehouseId }).IsUnique();

        builder.HasOne(a => a.Employee)
            .WithMany(e => e.WarehouseAssignments)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Warehouse)
            .WithMany(w => w.EmployeeAssignments)
            .HasForeignKey(a => a.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
