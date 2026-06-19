using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Typed HTTP client for WarehouseKG API integration tests.
/// </summary>
public class WarehouseKgClient
{
    private readonly HttpClient _http;

    public WarehouseKgClient(HttpClient http)
    {
        _http = http;
    }

    // ─── Auth ─────────────────────────────────────────────────────────────

    public async Task LoginAsync(string userName, string password)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/auth/login", new
        {
            userName,
            password
        });
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        // Also set tenant header
        if (!_http.DefaultRequestHeaders.Contains("X-Tenant-Id"))
            _http.DefaultRequestHeaders.Add("X-Tenant-Id", "ffffffff-ffff-ffff-ffff-ffffffffffff");
    }

    /// <summary>Create a user with specified roles (requires admin). Returns the new user ID.</summary>
    public async Task<string> CreateUserAsync(string userName, string password, List<string> roles)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/users", new
        {
            userName,
            email = $"{userName}@test.local",
            password,
            roles
        });
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>Login as a specific user and return a new client authenticated as that user.</summary>
    public async Task<WarehouseKgClient> LoginAsUserAsync(string userName, string password)
    {
        var http = new HttpClient { BaseAddress = _http.BaseAddress };
        var client = new WarehouseKgClient(http);
        await client.LoginAsync(userName, password);
        return client;
    }

    /// <summary>Raw HTTP GET — returns full response for permission testing.</summary>
    public async Task<HttpResponseMessage> GetRawAsync(string url)
    {
        return await _http.GetAsync(url);
    }

    /// <summary>Raw HTTP POST — returns full response for permission testing.</summary>
    public async Task<HttpResponseMessage> PostRawAsync(string url, object? body = null)
    {
        if (body is null)
            return await _http.PostAsync(url, null);
        return await _http.PostAsJsonAsync(url, body);
    }

    // ─── Stock Receipts ──────────────────────────────────────────────────

    public async Task<string> CreateReceiptAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/stock-receipts", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<JsonElement> GetReceiptsAsync()
    {
        var response = await _http.GetAsync("/api/v1/stock-receipts");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task CompleteReceiptAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/stock-receipts/{id}/complete", null);
        response.EnsureSuccessStatusCode();
    }

    // ─── Pick Orders ─────────────────────────────────────────────────────

    public async Task<string> CreatePickOrderAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/pick-orders", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task CompletePickOrderAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/pick-orders/{id}/complete", null);
        response.EnsureSuccessStatusCode();
    }

    // ─── Pack Orders ─────────────────────────────────────────────────────

    public async Task<string> CreatePackOrderAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/pack-orders", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task CompletePackOrderAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/pack-orders/{id}/complete", null);
        response.EnsureSuccessStatusCode();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    public async Task<JsonElement> GetWarehousesAsync()
    {
        var response = await _http.GetAsync("/api/v1/warehouses");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task<JsonElement> GetInventoryItemsAsync()
    {
        var response = await _http.GetAsync("/api/v1/inventory-items");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    // ─── Stock Transfers ─────────────────────────────────────────────────

    public async Task<string> CreateStockTransferAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/stock-transfers", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task CompleteStockTransferAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/stock-transfers/{id}/complete", null);
        response.EnsureSuccessStatusCode();
    }

    // ─── Purchase Orders ─────────────────────────────────────────────────

    public async Task<string> CreatePurchaseOrderAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/purchase-orders", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task SubmitPurchaseOrderAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/purchase-orders/{id}/submit", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReceivePurchaseOrderAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/purchase-orders/{id}/receive", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<JsonElement> GetSuppliersAsync()
    {
        var response = await _http.GetAsync("/api/v1/suppliers");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task<JsonElement> GetCustomersAsync()
    {
        var response = await _http.GetAsync("/api/v1/customers");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task<JsonElement> GetCategoriesAsync()
    {
        var response = await _http.GetAsync("/api/v1/item-categories");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task<JsonElement> GetUnitsOfMeasureAsync()
    {
        var response = await _http.GetAsync("/api/v1/units-of-measure");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task<string> CreateCustomerAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/customers", body);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> CreateInventoryItemAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/inventory-items", body);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStringAsync();
    }

    // ─── Sales Orders ────────────────────────────────────────────────────

    public async Task<string> CreateSalesOrderAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/sales-orders", body);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task ConfirmSalesOrderAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/sales-orders/{id}/confirm", null);
        await EnsureSuccessAsync(response);
    }

    public async Task ShipSalesOrderAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/sales-orders/{id}/ship", null);
        await EnsureSuccessAsync(response);
    }

    // ─── Stock Adjustments ───────────────────────────────────────────────

    public async Task<string> CreateStockAdjustmentAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/stock-adjustments", body);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task CompleteStockAdjustmentAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/stock-adjustments/{id}/complete", null);
        await EnsureSuccessAsync(response);
    }

    // ─── Stock Audits ────────────────────────────────────────────────────

    public async Task<string> CreateStockAuditAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/stock-audits", body);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task CompleteStockAuditAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/stock-audits/{id}/complete", null);
        await EnsureSuccessAsync(response);
    }

    public async Task CancelStockAuditAsync(string id)
    {
        var response = await _http.PostAsync($"/api/v1/stock-audits/{id}/cancel", null);
        await EnsureSuccessAsync(response);
    }

    public async Task<JsonElement> GetStockAuditsAsync()
    {
        var response = await _http.GetAsync("/api/v1/stock-audits");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task DeleteStockAuditAsync(string id)
    {
        var response = await _http.DeleteAsync($"/api/v1/stock-audits/{id}");
        await EnsureSuccessAsync(response);
    }

    // ─── Reports ─────────────────────────────────────────────────────────

    public async Task<JsonElement> GetWarehouseStockAsync(Guid warehouseId, string? dateFrom = null, string? dateTo = null)
    {
        var url = $"/api/v1/reports/warehouse-stock?warehouseId={warehouseId}";
        if (dateFrom != null) url += $"&dateFrom={dateFrom}";
        if (dateTo != null) url += $"&dateTo={dateTo}";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task<JsonElement> GetItemMovementsAsync(Guid itemId, Guid warehouseId)
    {
        var url = $"/api/v1/reports/item-movements?itemId={itemId}&warehouseId={warehouseId}";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Response status code {(int)response.StatusCode}: {body}");
        }
    }

    private record LoginResponse(string AccessToken);
}
