using System.Net;
using System.Text.Json;

namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Integration tests for Superadmin tenant management.
/// Tests: tenant CRUD, suspend/activate, authorization checks.
/// </summary>
[Collection("IntegrationTests")]
public class SuperadminTests
{
    private readonly SharedFixture _fixture;

    public SuperadminTests(SharedFixture fixture)
    {
        _fixture = fixture;
    }

    private WarehouseKgClient Admin => _fixture.Client;

    private async Task<WarehouseKgClient> SuperadminClient()
    {
        return await _fixture.CreateClientAsync("superadmin", "Super1234!");
    }

    // ─── Authorization Checks ────────────────────────────────────────────

    [Fact]
    public async Task RegularAdmin_CannotAccess_Tenants()
    {
        var response = await Admin.GetRawAsync("/api/v1/tenants");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Superadmin_CanAccess_Tenants()
    {
        var sa = await SuperadminClient();
        var response = await sa.GetRawAsync("/api/v1/tenants");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ─── CRUD: Create Tenant ─────────────────────────────────────────────

    [Fact]
    public async Task Superadmin_CanCreate_Tenant()
    {
        var sa = await SuperadminClient();
        var slug = $"test-{Guid.NewGuid():N}"[..12];

        var response = await sa.PostRawAsync("/api/v1/tenants", new
        {
            name = "Integration Test Tenant",
            slug = slug,
            contactEmail = "test@example.com",
            contactPhone = "+996555123456",
            defaultCurrency = "KGS",
            adminUserName = $"admin-{Guid.NewGuid():N}"[..12],
            adminEmail = "admin@testtenant.local",
            adminPassword = "Test1234!",
            seedDemoData = false
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetString()!;
        Assert.NotEmpty(id);
        Assert.Equal("Integration Test Tenant", body.GetProperty("name").GetString());
        Assert.Equal(slug, body.GetProperty("slug").GetString());
        Assert.Equal(1, body.GetProperty("status").GetInt32()); // Active
        Assert.Equal(1, body.GetProperty("userCount").GetInt32()); // Admin user created
    }

    [Fact]
    public async Task Superadmin_CreateTenant_WithDemoData()
    {
        var sa = await SuperadminClient();
        var slug = $"demo-{Guid.NewGuid():N}"[..12];
        var adminUserName = $"adm-{Guid.NewGuid():N}"[..10];

        var response = await sa.PostRawAsync("/api/v1/tenants", new
        {
            name = "Demo Tenant",
            slug = slug,
            contactEmail = "demo@example.com",
            contactPhone = null as string,
            defaultCurrency = "USD",
            adminUserName = adminUserName,
            adminEmail = "adm@demo.local",
            adminPassword = "Demo1234!",
            seedDemoData = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var tenantId = body.GetProperty("id").GetString()!;

        // Verify the tenant admin can login
        var tenantAdmin = await _fixture.CreateClientAsync(adminUserName, "Demo1234!");
        Assert.NotNull(tenantAdmin);

        // Verify tenant details via superadmin
        var getResponse = await sa.GetRawAsync($"/api/v1/tenants/{tenantId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var detail = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Demo Tenant", detail.GetProperty("name").GetString());
        Assert.Equal("USD", detail.GetProperty("defaultCurrency").GetString());
        Assert.Equal(1, detail.GetProperty("status").GetInt32());
    }

    // ─── CRUD: Get Tenants ───────────────────────────────────────────────

    [Fact]
    public async Task Superadmin_CanList_Tenants()
    {
        var sa = await SuperadminClient();
        var response = await sa.GetRawAsync("/api/v1/tenants");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tenants = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(tenants.GetArrayLength() > 0, "Should return at least one tenant");
    }

    [Fact]
    public async Task Superadmin_GetTenantById_NotFound()
    {
        var sa = await SuperadminClient();
        var response = await sa.GetRawAsync($"/api/v1/tenants/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── CRUD: Update Tenant ─────────────────────────────────────────────

    [Fact]
    public async Task Superadmin_CanUpdate_Tenant()
    {
        var sa = await SuperadminClient();

        // First create a tenant
        var slug = $"upd-{Guid.NewGuid():N}"[..12];
        var createResp = await sa.PostRawAsync("/api/v1/tenants", new
        {
            name = "Update Test Tenant",
            slug = slug,
            defaultCurrency = "KGS",
            adminUserName = $"upd-{Guid.NewGuid():N}"[..12],
            adminEmail = "upd@test.local",
            adminPassword = "Test1234!",
            seedDemoData = false
        });
        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString()!;

        // Update it
        var updateResp = await sa.PutRawAsync($"/api/v1/tenants/{id}", new
        {
            name = "Updated Tenant Name",
            slug = slug,
            contactEmail = "updated@example.com",
            contactPhone = "+996555999999",
            defaultCurrency = "RUB",
            maxUsers = 50,
            enabledModules = "inventory,crm"
        });

        // PUT returns NoContent
        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        // Verify update
        var getResp = await sa.GetRawAsync($"/api/v1/tenants/{id}");
        var updated = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated Tenant Name", updated.GetProperty("name").GetString());
        Assert.Equal("updated@example.com", updated.GetProperty("contactEmail").GetString());
        Assert.Equal("RUB", updated.GetProperty("defaultCurrency").GetString());
    }

    [Fact]
    public async Task Superadmin_UpdateTenant_NotFound()
    {
        var sa = await SuperadminClient();
        var response = await sa.PutRawAsync($"/api/v1/tenants/{Guid.NewGuid()}", new
        {
            name = "Ghost",
            slug = "ghost",
            contactEmail = null as string,
            contactPhone = null as string,
            defaultCurrency = "KGS",
            maxUsers = (int?)null,
            enabledModules = null as string
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── Suspend / Activate ──────────────────────────────────────────────

    [Fact]
    public async Task Superadmin_CanSuspendAndActivate_Tenant()
    {
        var sa = await SuperadminClient();

        // Create a tenant
        var slug = $"sus-{Guid.NewGuid():N}"[..12];
        var createResp = await sa.PostRawAsync("/api/v1/tenants", new
        {
            name = "Suspend Test Tenant",
            slug = slug,
            defaultCurrency = "KGS",
            adminUserName = $"sus-{Guid.NewGuid():N}"[..12],
            adminEmail = "sus@test.local",
            adminPassword = "Test1234!",
            seedDemoData = false
        });
        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString()!;
        Assert.Equal(1, created.GetProperty("status").GetInt32()); // Active

        // Suspend
        var suspendResp = await sa.PostRawAsync($"/api/v1/tenants/{id}/suspend");
        Assert.Equal(HttpStatusCode.NoContent, suspendResp.StatusCode);

        // Verify suspended
        var getResp1 = await sa.GetRawAsync($"/api/v1/tenants/{id}");
        var suspended = await getResp1.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, suspended.GetProperty("status").GetInt32()); // Suspended

        // Activate
        var activateResp = await sa.PostRawAsync($"/api/v1/tenants/{id}/activate");
        Assert.Equal(HttpStatusCode.NoContent, activateResp.StatusCode);

        // Verify active again
        var getResp2 = await sa.GetRawAsync($"/api/v1/tenants/{id}");
        var reactivated = await getResp2.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, reactivated.GetProperty("status").GetInt32()); // Active
    }

    [Fact]
    public async Task Superadmin_SuspendTenant_NotFound()
    {
        var sa = await SuperadminClient();
        var response = await sa.PostRawAsync($"/api/v1/tenants/{Guid.NewGuid()}/suspend");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Superadmin_ActivateTenant_NotFound()
    {
        var sa = await SuperadminClient();
        var response = await sa.PostRawAsync($"/api/v1/tenants/{Guid.NewGuid()}/activate");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── Password Reset ─────────────────────────────────────────────────

    private async Task<(string tenantId, string adminUserId, string adminUserName)> CreateTenantForTest(WarehouseKgClient sa)
    {
        var adminUserName = $"pwd-{Guid.NewGuid():N}"[..12];
        var slug = $"pwd-{Guid.NewGuid():N}"[..12];

        var response = await sa.PostRawAsync("/api/v1/tenants", new
        {
            name = "Password Test Tenant",
            slug = slug,
            defaultCurrency = "KGS",
            adminUserName = adminUserName,
            adminEmail = $"{adminUserName}@test.local",
            adminPassword = "OldPass123!",
            seedDemoData = false
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var tenantId = body.GetProperty("id").GetString()!;
        var adminUserId = body.GetProperty("adminUserId").GetString()!;
        return (tenantId, adminUserId, adminUserName);
    }

    [Fact]
    public async Task Superadmin_CanReset_TenantUserPassword()
    {
        var sa = await SuperadminClient();
        var (tenantId, adminUserId, adminUserName) = await CreateTenantForTest(sa);

        // Verify the admin user can login with old password
        var adminClient = await _fixture.CreateClientAsync(adminUserName, "OldPass123!");
        Assert.NotNull(adminClient);

        // Reset the password
        var resetResp = await sa.PutRawAsync(
            $"/api/v1/tenants/{tenantId}/users/{adminUserId}/password",
            new { newPassword = "NewPass456!" });
        Assert.Equal(HttpStatusCode.NoContent, resetResp.StatusCode);

        // Old password should no longer work
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _fixture.CreateClientAsync(adminUserName, "OldPass123!"));

        // New password should work
        var newLogin = await _fixture.CreateClientAsync(adminUserName, "NewPass456!");
        Assert.NotNull(newLogin);
    }

    [Fact]
    public async Task Superadmin_ResetPassword_UserNotFound_Returns404()
    {
        var sa = await SuperadminClient();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var response = await sa.PutRawAsync(
            $"/api/v1/tenants/{tenantId}/users/{userId}/password",
            new { newPassword = "NewPass123!" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegularAdmin_CannotReset_TenantUserPassword()
    {
        // Admin should not be able to use superadmin password reset endpoint
        var response = await Admin.PutRawAsync(
            $"/api/v1/tenants/{Guid.NewGuid()}/users/{Guid.NewGuid()}/password",
            new { newPassword = "NewPass123!" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
