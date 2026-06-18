namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Base class for integration tests. Creates a WebApplicationFactory
/// pointed at the test database and provides an authenticated client.
///
/// Prerequisites:
///   1. PostgreSQL must be running (docker compose up -d postgres)
///   2. A test database "WAREHOUSEKG_TEST" must exist.
///      Create it: docker exec wkg-postgres psql -U postgres -c "CREATE DATABASE \"WAREHOUSEKG_TEST\""
///   3. Migrations are applied automatically by the factory on first run.
///   4. Seed user admin / Admin1234! is created by the app on startup.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected WarehouseKgClient Client { get; private set; } = null!;
    private TestWebApplicationFactory _factory = null!;

    // Use the same PostgreSQL instance but a separate test database
    private const string ConnectionString =
        "Server=localhost;Port=15432;Database=WAREHOUSEKG_TEST;User ID=postgres;Password=P@@ssw0rd!;Enlist=true;Maximum Pool Size=10;Minimum Pool Size=2;Timeout=30;Command Timeout=60";

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory(ConnectionString);
        var http = _factory.CreateClient();
        Client = new WarehouseKgClient(http);
        await Client.LoginAsync("admin", "Admin1234!");
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    /// <summary>Helper: get first warehouse ID from the API.</summary>
    protected async Task<string> GetFirstWarehouseIdAsync()
    {
        var warehouses = await Client.GetWarehousesAsync();
        return warehouses[0].GetProperty("id").GetString()!;
    }

    /// <summary>Helper: get first inventory item ID from the API.</summary>
    protected async Task<string> GetFirstItemIdAsync()
    {
        var items = await Client.GetInventoryItemsAsync();
        return items[0].GetProperty("id").GetString()!;
    }
}
