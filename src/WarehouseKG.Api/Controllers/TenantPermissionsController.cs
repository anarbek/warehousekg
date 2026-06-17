using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Admin-only: manages tenant-scoped permission overrides for roles.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
[ApiController]
[Route("api/v1/tenant-permissions")]
public class TenantPermissionsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public TenantPermissionsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TenantPermissionDto>>> GetAll(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var permissions = await _context.TenantPermissions
            .Where(p => p.TenantId == tenantId)
            .ToListAsync(ct);

        return Ok(permissions.Select(Map).ToList());
    }

    [HttpPut]
    public async Task<IActionResult> Upsert(TenantPermissionDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        UpsertSingle(dto, tenantId);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpsert(List<TenantPermissionDto> dtos, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        foreach (var dto in dtos)
            UpsertSingle(dto, tenantId);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    private void UpsertSingle(TenantPermissionDto dto, Guid tenantId)
    {
        var existing = _context.TenantPermissions
            .FirstOrDefault(p =>
                p.TenantId == tenantId &&
                p.RoleName == dto.RoleName &&
                p.Resource == dto.Resource);

        if (existing != null)
        {
            existing.CanRead = dto.CanRead;
            existing.CanWrite = dto.CanWrite;
            existing.CanDelete = dto.CanDelete;
        }
        else
        {
            _context.TenantPermissions.Add(new TenantPermission
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RoleName = dto.RoleName,
                Resource = dto.Resource,
                CanRead = dto.CanRead,
                CanWrite = dto.CanWrite,
                CanDelete = dto.CanDelete
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var p = await _context.TenantPermissions.FindAsync([id], ct);
        if (p == null || p.TenantId != GetTenantId()) return NotFound();
        _context.TenantPermissions.Remove(p);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("roles")]
    public ActionResult<List<string>> GetRoles()
    {
        return Ok(Roles.All.ToList());
    }

    [HttpGet("resources")]
    public ActionResult<List<ResourceInfo>> GetResources()
    {
        return Ok(Resources.All.Select(r => new ResourceInfo { Key = r, Label = r }).ToList());
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("tenant_id")?.Value;
        return Guid.Parse(claim!);
    }

    private static TenantPermissionDto Map(TenantPermission p) => new()
    {
        Id = p.Id,
        RoleName = p.RoleName,
        Resource = p.Resource,
        CanRead = p.CanRead,
        CanWrite = p.CanWrite,
        CanDelete = p.CanDelete
    };
}

public class TenantPermissionDto
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanDelete { get; set; }
}

public class ResourceInfo
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
