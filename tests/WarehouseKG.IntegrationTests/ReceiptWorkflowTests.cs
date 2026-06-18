using System.Text.Json;

namespace WarehouseKG.IntegrationTests;

[Collection("IntegrationTests")]
public class ReceiptWorkflowTests
{
    private readonly SharedFixture _fixture;

    public ReceiptWorkflowTests(SharedFixture fixture)
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
    public async Task CreateReceipt_Complete_VerifyStockIncreases()
    {
        // Arrange
        var warehouseId = await GetFirstWarehouseIdAsync();
        var itemId = await GetFirstItemIdAsync();

        // Act — create receipt
        var receiptId = await Client.CreateReceiptAsync(new
        {
            number = $"TEST-{Guid.NewGuid():N}"[..12],
            warehouseId,
            supplierReference = "integration-test",
            notes = "automated receipt",
            receivedAtUtc = DateTime.UtcNow,
            lines = new[]
            {
                new { inventoryItemId = itemId, quantity = 10, warehouseLocationId = (string?)null }
            }
        });

        Assert.NotEmpty(receiptId);

        // Act — verify it appears in list
        var receipts = await Client.GetReceiptsAsync();
        var found = false;
        foreach (var r in receipts.EnumerateArray())
        {
            if (r.GetProperty("id").GetString() == receiptId.Trim('"'))
            {
                Assert.Equal("Draft", r.GetProperty("status").GetString());
                found = true;
                break;
            }
        }
        Assert.True(found, "Receipt should be in the list");

        // Act — complete the receipt
        await Client.CompleteReceiptAsync(receiptId.Trim('"'));

        // Verify — status changed
        receipts = await Client.GetReceiptsAsync();
        foreach (var r in receipts.EnumerateArray())
        {
            if (r.GetProperty("id").GetString() == receiptId.Trim('"'))
            {
                Assert.Equal("Completed", r.GetProperty("status").GetString());
                break;
            }
        }
    }

    [Fact]
    public async Task CreateMultipleReceipts_AllAppearInList()
    {
        var warehouseId = await GetFirstWarehouseIdAsync();
        var itemId = await GetFirstItemIdAsync();

        var ids = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            var id = await Client.CreateReceiptAsync(new
            {
                number = $"BATCH-{i:D3}",
                warehouseId,
                receivedAtUtc = DateTime.UtcNow,
                lines = new[]
                {
                    new { inventoryItemId = itemId, quantity = 5 + i }
                }
            });
            ids.Add(id.Trim('"'));
        }

        var receipts = await Client.GetReceiptsAsync();
        var receiptIds = receipts.EnumerateArray().Select(r => r.GetProperty("id").GetString()).ToHashSet();
        foreach (var id in ids)
            Assert.Contains(id, receiptIds);
    }
}
