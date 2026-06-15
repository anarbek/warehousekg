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

    public WarehouseKgDbContext(
        DbContextOptions<WarehouseKgDbContext> options,
        ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
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

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

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
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantId();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
