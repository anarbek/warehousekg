using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class EmployeeShiftAssignmentConfiguration : IEntityTypeConfiguration<EmployeeShiftAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeShiftAssignment> builder)
    {
        builder.ToTable("employee_shift_assignments");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.TenantId, a.EmployeeId, a.EffectiveFromUtc }).IsUnique();

        builder.HasOne(a => a.Employee)
            .WithMany(e => e.ShiftAssignments)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Shift)
            .WithMany(s => s.EmployeeAssignments)
            .HasForeignKey(a => a.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
