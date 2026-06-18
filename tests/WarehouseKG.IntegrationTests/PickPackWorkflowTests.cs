namespace WarehouseKG.IntegrationTests;

[Collection("IntegrationTests")]
public class PickPackWorkflowTests
{
    private readonly SharedFixture _fixture;

    public PickPackWorkflowTests(SharedFixture fixture)
    {
        _fixture = fixture;
    }

    private WarehouseKgClient Client => _fixture.Client;

    private async Task<string> GetFirstWarehouseIdAsync()
    {
        var warehouses = await Client.GetWarehousesAsync();
        return warehouses[0].GetProperty("id").GetString()!;
    }

    private async Task<string> GetFirstItemIdAsync()
    {
        var items = await Client.GetInventoryItemsAsync();
        return items[0].GetProperty("id").GetString()!;
    }
    [Fact]
    public async Task CreatePickOrder_ThenPackOrder_FullFlow()
    {
        var warehouseId = await GetFirstWarehouseIdAsync();
        var itemId = await GetFirstItemIdAsync();

        // 1. First create + complete a receipt to have stock
        var receiptId = await Client.CreateReceiptAsync(new
        {
            number = $"STOCK-{Guid.NewGuid():N}"[..12],
            warehouseId,
            receivedAtUtc = DateTime.UtcNow,
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 50 }
            }
        });
        await Client.CompleteReceiptAsync(receiptId.Trim('"'));

        // 2. Create a pick order
        var pickId = await Client.CreatePickOrderAsync(new
        {
            number = $"PICK-{Guid.NewGuid():N}"[..10],
            warehouseId,
            plannedPickDate = DateTime.UtcNow.AddDays(1),
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 20 }
            }
        });
        Assert.NotEmpty(pickId);

        // 3. Complete the pick order
        await Client.CompletePickOrderAsync(pickId.Trim('"'));

        // 4. Create a pack order referencing the pick order
        var packId = await Client.CreatePackOrderAsync(new
        {
            number = $"PACK-{Guid.NewGuid():N}"[..10],
            warehouseId,
            pickOrderId = pickId.Trim('"'),
            actualPackDate = DateTime.UtcNow,
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 20, packageLabel = "BOX-001" }
            }
        });
        Assert.NotEmpty(packId);

        // 5. Complete the pack order
        await Client.CompletePackOrderAsync(packId.Trim('"'));
    }

    [Fact]
    public async Task PickOrder_WithPlannedDate_ShowsInList()
    {
        var warehouseId = await GetFirstWarehouseIdAsync();
        var itemId = await GetFirstItemIdAsync();

        var plannedDate = DateTime.UtcNow.AddDays(3);
        var pickId = await Client.CreatePickOrderAsync(new
        {
            number = $"DATE-{Guid.NewGuid():N}"[..10],
            warehouseId,
            plannedPickDate = plannedDate,
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 5 }
            }
        });

        Assert.NotEmpty(pickId);
    }
}
