using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Common;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Infrastructure.Identity;

namespace WarehouseKG.Infrastructure.Persistence;

public class WarehouseKgDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUser;

    public WarehouseKgDbContext(
        DbContextOptions<WarehouseKgDbContext> options,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUser)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _currentUser = currentUser;
    }

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();

    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();

    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<StockReceipt> StockReceipts => Set<StockReceipt>();

    public DbSet<StockReceiptLine> StockReceiptLines => Set<StockReceiptLine>();

    public DbSet<PickOrder> PickOrders => Set<PickOrder>();

    public DbSet<PickOrderLine> PickOrderLines => Set<PickOrderLine>();

    public DbSet<PackOrder> PackOrders => Set<PackOrder>();

    public DbSet<PackOrderLine> PackOrderLines => Set<PackOrderLine>();

    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();

    public DbSet<StockTransferLine> StockTransferLines => Set<StockTransferLine>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

    public DbSet<StockAdjustmentLine> StockAdjustmentLines => Set<StockAdjustmentLine>();

    public DbSet<StockAudit> StockAudits => Set<StockAudit>();

    public DbSet<StockAuditLine> StockAuditLines => Set<StockAuditLine>();

    public DbSet<TenantPermission> TenantPermissions => Set<TenantPermission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Shift> Shifts => Set<Shift>();

    public DbSet<EmployeeShiftAssignment> EmployeeShiftAssignments => Set<EmployeeShiftAssignment>();

    public DbSet<EmployeeWarehouseAssignment> EmployeeWarehouseAssignments => Set<EmployeeWarehouseAssignment>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    public Guid CurrentTenantId => _tenantProvider.GetTenantId();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseKgDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(BuildTenantFilter(entityType.ClrType));
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantId();
        ApplyAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantId();
        ApplyAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTime.UtcNow;
        var user = _currentUser.UserName ?? "system";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = user;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
            }

            entry.Entity.UpdatedAt = now;
            entry.Entity.UpdatedBy = user;
        }
    }

    private void ApplyTenantId()
    {
        var tenantId = CurrentTenantId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = tenantId;
            }
        }
    }

    // Builds: entity => EF.Property<Guid>(entity, "TenantId") == this.CurrentTenantId
    // Referencing the context instance member keeps the filter parameterized so the
    // current tenant is re-evaluated on every query rather than baked in at model build.
    private LambdaExpression BuildTenantFilter(Type entityClrType)
    {
        var parameter = Expression.Parameter(entityClrType, "e");

        var tenantProperty = Expression.Property(parameter, nameof(BaseEntity.TenantId));

        var currentTenant = Expression.Property(
            Expression.Constant(this),
            nameof(CurrentTenantId));

        var body = Expression.Equal(tenantProperty, currentTenant);

        return Expression.Lambda(body, parameter);
    }
}
