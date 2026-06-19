using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Identity;
using WarehouseKG.Infrastructure.Identity;

namespace WarehouseKG.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, IApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var users = await _userManager.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.UserName)
            .ToListAsync(ct);

        var userIds = users.Select(u => u.Id).ToList();
        var employees = await _context.Employees
            .Where(e => e.TenantId == tenantId && e.ApplicationUserId != null && userIds.Contains(e.ApplicationUserId.Value))
            .ToDictionaryAsync(e => e.ApplicationUserId!.Value, ct);

        var result = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var emp = employees.GetValueOrDefault(u.Id);
            result.Add(new UserDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Roles = roles.ToList(),
                EmployeeId = emp?.Id,
                EmployeeName = emp != null ? $"{emp.LastName} {emp.FirstName}" : null
            });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            TenantId = tenantId
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        if (dto.Roles.Count > 0)
            await _userManager.AddToRolesAsync(user, dto.Roles);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user.Id);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.TenantId != GetTenantId()) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var emp = await _context.Employees
            .FirstOrDefaultAsync(e => e.ApplicationUserId == id, ct);

        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            EmployeeId = emp?.Id,
            EmployeeName = emp != null ? $"{emp.LastName} {emp.FirstName}" : null
        });
    }

    [HttpPut("{id:guid}/employee")]
    public async Task<IActionResult> LinkEmployee(Guid id, LinkEmployeeDto dto, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.TenantId != GetTenantId()) return NotFound();

        var tenantId = GetTenantId();

        // Unlink any existing employee for this user
        var existing = await _context.Employees
            .FirstOrDefaultAsync(e => e.ApplicationUserId == id, ct);
        if (existing != null)
            existing.ApplicationUserId = null;

        // Link new employee
        if (dto.EmployeeId.HasValue)
        {
            var emp = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId.Value && e.TenantId == tenantId, ct);
            if (emp == null) return NotFound("Employee not found");

            // Ensure employee isn't already linked to another user
            if (emp.ApplicationUserId != null && emp.ApplicationUserId != id)
                return Conflict("Employee is already linked to another user");

            emp.ApplicationUserId = id;
        }

        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> UpdateRoles(Guid id, List<string> roles, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.TenantId != GetTenantId()) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (roles.Count > 0)
            await _userManager.AddToRolesAsync(user, roles);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.TenantId != GetTenantId()) return NotFound();

        await _userManager.DeleteAsync(user);
        return NoContent();
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordDto dto, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.TenantId != GetTenantId()) return NotFound();

        // Remove existing password and set new one (admin-forced reset)
        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return BadRequest(removeResult.Errors.Select(e => e.Description));

        var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
        if (!addResult.Succeeded)
            return BadRequest(addResult.Errors.Select(e => e.Description));

        return NoContent();
    }

    [HttpGet("roles")]
    public ActionResult<List<string>> GetAvailableRoles()
    {
        return Ok(Roles.All.ToList());
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("tenant_id")?.Value;
        return Guid.Parse(claim!);
    }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
}

public class CreateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public Guid? EmployeeId { get; set; }
}

public class ChangePasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class LinkEmployeeDto
{
    public Guid? EmployeeId { get; set; }
}
