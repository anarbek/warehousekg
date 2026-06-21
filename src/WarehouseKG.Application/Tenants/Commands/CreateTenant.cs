using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Tenants.DTOs;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Application.Tenants.Commands;

public record CreateTenantCommand(
    string Name,
    string Slug,
    string? ContactEmail,
    string? ContactPhone,
    string DefaultCurrency,
    string AdminUserName,
    string AdminEmail,
    string AdminPassword,
    bool SeedDemoData = false
) : IRequest<TenantDto>;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreateTenantCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Create Tenant record
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            DefaultCurrency = request.DefaultCurrency,
            Status = TenantStatus.Active,
            TenantId = Guid.Empty, // Tenants are global, not scoped to another tenant
        };
        _context.Tenants.Add(tenant);

        // 2. Save tenant first to get the ID
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Create admin user for the new tenant
        var createResult = await _identityService.CreateUserAsync(
            request.AdminUserName,
            request.AdminEmail,
            request.AdminPassword,
            tenant.Id,
            Roles.Admin,
            cancellationToken);

        if (!createResult.Succeeded)
        {
            // Rollback tenant creation by removing it
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", createResult.Errors)}");
        }

        // 4. Seed default data if requested
        if (request.SeedDemoData)
        {
            await SeedDemoDataAsync(tenant.Id, cancellationToken);
        }

        // 5. Return tenant info
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            DefaultCurrency = tenant.DefaultCurrency,
            Status = (int)tenant.Status,
            MaxUsers = tenant.MaxUsers,
            EnabledModules = tenant.EnabledModules,
            CreatedAt = tenant.CreatedAt,
            UserCount = 1, // Just created the admin user
            AdminUserId = createResult.User!.Id,
            AdminUserName = request.AdminUserName
        };
    }

    private async Task SeedDemoDataAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        // Seed default item categories
        var categories = new[]
        {
            new ItemCategory { Id = Guid.NewGuid(), Code = "GEN", Name = "Общее", IsActive = true, TenantId = tenantId },
            new ItemCategory { Id = Guid.NewGuid(), Code = "ELEC", Name = "Электроника", IsActive = true, TenantId = tenantId },
            new ItemCategory { Id = Guid.NewGuid(), Code = "FOOD", Name = "Продукты питания", IsActive = true, TenantId = tenantId },
        };
        _context.ItemCategories.AddRange(categories);

        // Seed default units of measure
        var units = new[]
        {
            new UnitOfMeasure { Id = Guid.NewGuid(), Code = "pcs", Name = "Штуки", TenantId = tenantId },
            new UnitOfMeasure { Id = Guid.NewGuid(), Code = "kg", Name = "Килограммы", TenantId = tenantId },
            new UnitOfMeasure { Id = Guid.NewGuid(), Code = "l", Name = "Литры", TenantId = tenantId },
        };
        _context.UnitsOfMeasure.AddRange(units);

        // Seed default warehouse
        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = "WH-001",
            Name = "Основной склад",
            Address = "Автоматически создан",
            IsActive = true,
            TenantId = tenantId,
        };
        _context.Warehouses.Add(warehouse);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
