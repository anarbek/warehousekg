using System.Text.Json;

namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Integration tests for the Invoice System (Phase 1 — Sales Invoice Core).
/// </summary>
[Collection("IntegrationTests")]
public class InvoiceWorkflowTests
{
    private readonly SharedFixture _fixture;

    public InvoiceWorkflowTests(SharedFixture fixture) => _fixture = fixture;

    private WarehouseKgClient Client => _fixture.Client;

    private static string Clean(string id) => id.Trim('"');

    private async Task<JsonElement> GetJsonAsync(string url)
    {
        var response = await Client.GetRawAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<(string warehouseId, string itemId, string customerId)> GetSeedIdsAsync()
    {
        var warehouses = await Client.GetWarehousesAsync();
        var warehouseId = warehouses[0].GetProperty("id").GetString()!;

        var items = await Client.GetInventoryItemsAsync();
        string itemId;
        if (items.GetArrayLength() == 0)
        {
            var categories = await Client.GetCategoriesAsync();
            var catId = categories[0].GetProperty("id").GetString()!;
            var uoms = await Client.GetUnitsOfMeasureAsync();
            var uomId = uoms[0].GetProperty("id").GetString()!;
            itemId = await Client.CreateInventoryItemAsync(new
            {
                sku = $"SKU-{Guid.NewGuid():N}"[..10],
                name = "Test Item for Invoice",
                categoryId = Guid.Parse(catId),
                unitOfMeasureId = Guid.Parse(uomId),
                warehouseId = Guid.Parse(warehouseId),
                quantityOnHand = 100m
            });
        }
        else
        {
            itemId = items[0].GetProperty("id").GetString()!;
        }

        var customers = await Client.GetCustomersAsync();
        string customerId;
        if (customers.GetArrayLength() == 0)
        {
            customerId = await Client.CreateCustomerAsync(new
            {
                code = $"CUST-{Guid.NewGuid():N}"[..6],
                name = "Test Customer LLC"
            });
        }
        else
        {
            customerId = customers[0].GetProperty("id").GetString()!;
        }

        return (warehouseId, Clean(itemId), Clean(customerId));
    }

    // ─── CRUD Tests ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateInvoice_ReturnsId_AndAppearsInList()
    {
        var (warehouseId, itemId, customerId) = await GetSeedIdsAsync();

        var id = await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new
                {
                    inventoryItemId = Guid.Parse(itemId),
                    quantity = 5m,
                    unitPrice = 100m,
                    taxRate = 0.12m
                }
            }
        });

        var invoiceId = Clean(id);
        Assert.NotEmpty(invoiceId);

        // Verify in list
        var invoices = await Client.GetInvoicesAsync();
        var found = false;
        foreach (var inv in invoices.EnumerateArray())
        {
            if (inv.GetProperty("id").GetString() == invoiceId)
            {
                Assert.Equal("Draft", inv.GetProperty("status").GetString());
                Assert.Contains("INV-", inv.GetProperty("number").GetString());
                found = true;
            }
        }
        Assert.True(found, "Invoice not found in list after creation");
    }

    [Fact]
    public async Task GetInvoiceById_ReturnsFullDetail_WithLines()
    {
        var (warehouseId, itemId, customerId) = await GetSeedIdsAsync();

        var id = await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new
                {
                    inventoryItemId = Guid.Parse(itemId),
                    quantity = 3m,
                    unitPrice = 250m,
                    taxRate = 0.12m
                }
            }
        });

        var invoiceId = Clean(id);
        var detail = await Client.GetInvoiceByIdAsync(Guid.Parse(invoiceId));

        Assert.Equal(invoiceId, detail.GetProperty("id").GetString());
        Assert.Equal("Draft", detail.GetProperty("status").GetString());
        Assert.Equal(customerId, detail.GetProperty("customerId").GetString());
        Assert.Equal(warehouseId, detail.GetProperty("warehouseId").GetString());

        var lines = detail.GetProperty("lines");
        Assert.Equal(1, lines.GetArrayLength());
        var line = lines[0];
        Assert.Equal(3m, line.GetProperty("quantity").GetDecimal());
        Assert.Equal(250m, line.GetProperty("unitPrice").GetDecimal());
        Assert.Equal(750m, line.GetProperty("lineTotal").GetDecimal());
    }

    [Fact]
    public async Task GetInvoiceById_NotFound_Returns404()
    {
        var response = await Client.GetRawAsync($"/api/v1/invoices/{Guid.NewGuid()}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteInvoice_RemovesFromList()
    {
        var (warehouseId, itemId, customerId) = await GetSeedIdsAsync();
        var id = await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new { inventoryItemId = Guid.Parse(itemId), quantity = 1m, unitPrice = 50m, taxRate = 0m }
            }
        });

        var invoiceId = Clean(id);
        await Client.DeleteInvoiceAsync(invoiceId);

        var response = await Client.GetRawAsync($"/api/v1/invoices/{invoiceId}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── Workflow Tests ───────────────────────────────────────────────

    [Fact]
    public async Task InvoiceWorkflow_DraftToSigned()
    {
        var (warehouseId, itemId, customerId) = await GetSeedIdsAsync();
        var id = await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new { inventoryItemId = Guid.Parse(itemId), quantity = 2m, unitPrice = 500m, taxRate = 0.12m }
            }
        });

        var invoiceId = Clean(id);
        var detail = await Client.GetInvoiceByIdAsync(Guid.Parse(invoiceId));
        Assert.Equal("Draft", detail.GetProperty("status").GetString());

        // Issue
        await Client.IssueInvoiceAsync(invoiceId);
        detail = await Client.GetInvoiceByIdAsync(Guid.Parse(invoiceId));
        Assert.Equal("Issued", detail.GetProperty("status").GetString());

        // Print
        await Client.PrintInvoiceAsync(invoiceId);
        detail = await Client.GetInvoiceByIdAsync(Guid.Parse(invoiceId));
        Assert.Equal("Printed", detail.GetProperty("status").GetString());

        // Sign
        await Client.SignInvoiceAsync(invoiceId, "John Doe");
        detail = await Client.GetInvoiceByIdAsync(Guid.Parse(invoiceId));
        Assert.Equal("Signed", detail.GetProperty("status").GetString());
        Assert.Equal("John Doe", detail.GetProperty("signedByName").GetString());
    }

    [Fact]
    public async Task InvoiceWorkflow_DraftToCancelled()
    {
        var (warehouseId, itemId, customerId) = await GetSeedIdsAsync();
        var id = await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new { inventoryItemId = Guid.Parse(itemId), quantity = 1m, unitPrice = 100m, taxRate = 0m }
            }
        });

        var invoiceId = Clean(id);

        await Client.IssueInvoiceAsync(invoiceId);
        var detail = await Client.GetInvoiceByIdAsync(Guid.Parse(invoiceId));
        Assert.Equal("Issued", detail.GetProperty("status").GetString());

        await Client.CancelInvoiceAsync(invoiceId);
        detail = await Client.GetInvoiceByIdAsync(Guid.Parse(invoiceId));
        Assert.Equal("Cancelled", detail.GetProperty("status").GetString());
    }

    [Fact]
    public async Task CancelInvoice_CannotCancelSigned()
    {
        var (warehouseId, itemId, customerId) = await GetSeedIdsAsync();
        var id = await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new { inventoryItemId = Guid.Parse(itemId), quantity = 1m, unitPrice = 100m, taxRate = 0m }
            }
        });

        var invoiceId = Clean(id);
        await Client.IssueInvoiceAsync(invoiceId);
        await Client.SignInvoiceAsync(invoiceId);

        // Try to cancel signed invoice — should fail
        var response = await Client.PostRawAsync($"/api/v1/invoices/{invoiceId}/cancel");
        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task IssueInvoice_CannotIssueTwice()
    {
        var (warehouseId, itemId, customerId) = await GetSeedIdsAsync();
        var id = await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new { inventoryItemId = Guid.Parse(itemId), quantity = 1m, unitPrice = 100m, taxRate = 0m }
            }
        });

        var invoiceId = Clean(id);
        await Client.IssueInvoiceAsync(invoiceId);

        var response = await Client.PostRawAsync($"/api/v1/invoices/{invoiceId}/issue");
        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    // ─── Filter Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task GetInvoices_FilterByStatus_ReturnsMatchingOnly()
    {
        var (warehouseId, itemId, customerId) = await GetSeedIdsAsync();

        // Create two invoices, issue one
        var id1 = Clean(await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new { inventoryItemId = Guid.Parse(itemId), quantity = 1m, unitPrice = 10m, taxRate = 0m }
            }
        }));

        var id2 = Clean(await Client.CreateInvoiceAsync(new
        {
            customerId = Guid.Parse(customerId),
            warehouseId = Guid.Parse(warehouseId),
            currency = "KGS",
            exchangeRate = 1m,
            lines = new[]
            {
                new { inventoryItemId = Guid.Parse(itemId), quantity = 2m, unitPrice = 20m, taxRate = 0m }
            }
        }));

        await Client.IssueInvoiceAsync(id2);

        // Filter by Issued
        var issuedResponse = await GetJsonAsync("/api/v1/invoices?status=Issued");
        foreach (var inv in issuedResponse.EnumerateArray())
        {
            Assert.Equal("Issued", inv.GetProperty("status").GetString());
        }

        // Filter by Draft
        var draftResponse = await GetJsonAsync("/api/v1/invoices?status=Draft");
        foreach (var inv in draftResponse.EnumerateArray())
        {
            Assert.Equal("Draft", inv.GetProperty("status").GetString());
        }
    }

    // ─── Auth/Permission Tests ─────────────────────────────────────────

    [Fact]
    public async Task Unauthenticated_CannotAccess_Invoices()
    {
        var rawClient = _fixture.CreateRawClient();
        var response = await rawClient.GetAsync("/api/v1/invoices");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
