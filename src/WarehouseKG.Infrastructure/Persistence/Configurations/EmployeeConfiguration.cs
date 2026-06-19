using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).HasMaxLength(32).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(128).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(128).IsRequired();
        builder.Property(e => e.MiddleName).HasMaxLength(128);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.Phone).HasMaxLength(64);
        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
        builder.HasIndex(e => e.ApplicationUserId).IsUnique().HasFilter(null); // one user per employee

        builder.HasOne(e => e.Position)
            .WithMany()
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
