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

    private record LoginResponse(string AccessToken);
}
