namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Shared test fixture: one WebApplicationFactory and seeded DB for all tests.
/// </summary>
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<SharedFixture>
{
}

public class SharedFixture : IAsyncLifetime
{
    public WarehouseKgClient Client { get; private set; } = null!;
    private TestWebApplicationFactory _factory = null!;

    private const string ConnectionString =
        "Server=localhost;Port=15432;Database=WAREHOUSEKG_TEST;User ID=postgres;Password=P@@ssw0rd!;Enlist=true;Maximum Pool Size=10;Minimum Pool Size=2;Timeout=30;Command Timeout=60";

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory(ConnectionString);
        var http = _factory.CreateClient();
        Client = new WarehouseKgClient(http);
        await Client.LoginAsync("admin", "Admin1234!");
    }

    /// <summary>Create a new client from the factory and login as the given user.</summary>
    public async Task<WarehouseKgClient> CreateClientAsync(string userName, string password)
    {
        var http = _factory.CreateClient();
        var client = new WarehouseKgClient(http);
        await client.LoginAsync(userName, password);
        return client;
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        await Task.CompletedTask;
    }
}
