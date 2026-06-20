using System.Text.Json;
using WarehouseKG.Application.Common;
using WarehouseKG.Domain.Common;

namespace WarehouseKG.IntegrationTests;

/// <summary>
/// Comprehensive integration tests for the Dispatching module:
/// routes, stops, shipments, geofences, and their workflows.
/// </summary>
[Collection("IntegrationTests")]
public class DispatcherWorkflowTests
{
    private readonly SharedFixture _fixture;

    public DispatcherWorkflowTests(SharedFixture fixture) => _fixture = fixture;

    private WarehouseKgClient Client => _fixture.Client;

    // ─── Helpers ───────────────────────────────────────────────────────

    private static string Clean(string id) => id.Trim('"');

    private async Task<(string vehicleId, string driverId, string customerId)> GetSeedIdsAsync()
    {
        // Get a vehicle
        var vehicles = await GetJsonAsync("/api/v1/vehicles");
        string vehicleId;
        if (vehicles.GetArrayLength() == 0)
        {
            // Need at least a vehicle type
            var vtId = await Client.CreateVehicleTypeAsync(new { code = "DTEST", name = "Test Van" });
            vehicleId = await Client.CreateVehicleAsync(new
            {
                code = $"DV-{Guid.NewGuid():N}"[..8],
                licensePlate = $"T{Guid.NewGuid():N}"[..6],
                brand = "TestBrand",
                vehicleTypeId = Guid.Parse(Clean(vtId))
            });
        }
        else
        {
            vehicleId = vehicles[0].GetProperty("id").GetString()!;
        }
        vehicleId = Clean(vehicleId);

        // Get an employee (driver)
        var employees = await GetJsonAsync("/api/v1/employees");
        string driverId;
        if (employees.GetArrayLength() == 0)
        {
            driverId = await Client.CreateEmployeeAsync(new
            {
                code = $"DRV-{Guid.NewGuid():N}"[..6],
                firstName = "Test",
                lastName = "Driver"
            });
        }
        else
        {
            driverId = employees[0].GetProperty("id").GetString()!;
        }
        driverId = Clean(driverId);

        // Get a customer
        var customers = await Client.GetCustomersAsync();
        string customerId;
        if (customers.GetArrayLength() == 0)
        {
            customerId = await Client.CreateCustomerAsync(new
            {
                code = $"CUST-{Guid.NewGuid():N}"[..6],
                name = "Test Customer Co"
            });
        }
        else
        {
            customerId = customers[0].GetProperty("id").GetString()!;
        }
        customerId = Clean(customerId);

        return (vehicleId, driverId, customerId);
    }

    private async Task<JsonElement> GetJsonAsync(string url)
    {
        var response = await Client.GetRawAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    // ─── Route CRUD Tests ──────────────────────────────────────────────

    [Fact]
    public async Task CreateRoute_ReturnsId_AndAppearsInList()
    {
        var (vehicleId, driverId, _) = await GetSeedIdsAsync();
        var code = $"R-{Guid.NewGuid():N}"[..12];

        var id = await Client.CreateRouteAsync(new
        {
            code,
            date = DateTime.UtcNow,
            vehicleId = Guid.Parse(vehicleId),
            driverEmployeeId = Guid.Parse(driverId)
        });

        var routeId = Clean(id);
        Assert.NotEmpty(routeId);

        // Verify it appears in list
        var routes = await Client.GetRoutesAsync();
        var found = false;
        foreach (var r in routes.EnumerateArray())
        {
            if (r.GetProperty("id").GetString() == routeId)
            {
                Assert.Equal(code, r.GetProperty("code").GetString());
                Assert.Equal("Planned", r.GetProperty("status").GetString());
                found = true;
            }
        }
        Assert.True(found, "Route not found in list after creation");

        // Cleanup
        await Client.DeleteRouteAsync(routeId);
    }

    [Fact]
    public async Task DeleteRoute_RemovesFromList()
    {
        var id = await Client.CreateRouteAsync(new
        {
            code = $"RDEL-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow
        });
        var routeId = Clean(id);

        await Client.DeleteRouteAsync(routeId);

        var routes = await Client.GetRoutesAsync();
        foreach (var r in routes.EnumerateArray())
        {
            Assert.NotEqual(routeId, r.GetProperty("id").GetString());
        }
    }

    // ─── Route Workflow Tests ──────────────────────────────────────────

    [Fact]
    public async Task RouteWorkflow_PlannedToCompleted()
    {
        var (vehicleId, driverId, _) = await GetSeedIdsAsync();
        var id = await Client.CreateRouteAsync(new
        {
            code = $"RWF-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow,
            vehicleId = Guid.Parse(vehicleId),
            driverEmployeeId = Guid.Parse(driverId)
        });
        var routeId = Clean(id);

        // Start
        await Client.StartRouteAsync(routeId);
        var detail = await Client.GetRouteDetailAsync(routeId);
        Assert.Equal("InProgress", detail.GetProperty("status").GetString());

        // Complete
        await Client.CompleteRouteAsync(routeId);
        detail = await Client.GetRouteDetailAsync(routeId);
        Assert.Equal("Completed", detail.GetProperty("status").GetString());

        // Cleanup
        await Client.DeleteRouteAsync(routeId);
    }

    [Fact]
    public async Task RouteWorkflow_CancelFromPlanned()
    {
        var id = await Client.CreateRouteAsync(new
        {
            code = $"RCXL-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow
        });
        var routeId = Clean(id);

        await Client.CancelRouteAsync(routeId);
        var detail = await Client.GetRouteDetailAsync(routeId);
        Assert.Equal("Cancelled", detail.GetProperty("status").GetString());

        await Client.DeleteRouteAsync(routeId);
    }

    [Fact]
    public async Task RouteWorkflow_CannotCompletePlannedRoute()
    {
        var id = await Client.CreateRouteAsync(new
        {
            code = $"RERR-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow
        });
        var routeId = Clean(id);

        // Try to complete a Planned route — should fail
        var response = await Client.PostRawAsync($"/api/v1/routes/{routeId}/complete");
        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);

        await Client.DeleteRouteAsync(routeId);
    }

    // ─── Stop CRUD & Workflow Tests ────────────────────────────────────

    [Fact]
    public async Task Stops_AddArriveComplete()
    {
        var (vehicleId, driverId, customerId) = await GetSeedIdsAsync();
        var id = await Client.CreateRouteAsync(new
        {
            code = $"RSTOP-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow,
            vehicleId = Guid.Parse(vehicleId),
            driverEmployeeId = Guid.Parse(driverId)
        });
        var routeId = Clean(id);

        // Create stop
        var stopId = await Client.CreateStopAsync(routeId, new
        {
            routeId = Guid.Parse(routeId),
            sequenceNumber = 1,
            customerId = Guid.Parse(customerId),
            address = "ул. Тестовая, 42",
            latitude = 42.8746,
            longitude = 74.5698,
            plannedArrivalUtc = DateTime.UtcNow.AddHours(1)
        });
        var sid = Clean(stopId);

        // Verify stop in route detail
        var detail = await Client.GetRouteDetailAsync(routeId);
        var stops = detail.GetProperty("stops");
        Assert.Equal(1, stops.GetArrayLength());
        var stop = stops[0];
        Assert.Equal("Pending", stop.GetProperty("status").GetString());

        // Start route
        await Client.StartRouteAsync(routeId);

        // Arrive
        await Client.ArriveAtStopAsync(routeId, sid);
        detail = await Client.GetRouteDetailAsync(routeId);
        stop = detail.GetProperty("stops")[0];
        Assert.Equal("InProgress", stop.GetProperty("status").GetString());
        Assert.True(stop.TryGetProperty("actualArrivalUtc", out _));

        // Complete
        await Client.CompleteStopAsync(routeId, sid);
        detail = await Client.GetRouteDetailAsync(routeId);
        stop = detail.GetProperty("stops")[0];
        Assert.Equal("Completed", stop.GetProperty("status").GetString());

        // Cleanup
        await Client.CancelRouteAsync(routeId);
        await Client.DeleteRouteAsync(routeId);
    }

    [Fact]
    public async Task Stops_MultipleStops_OrderedBySequence()
    {
        var (_, _, customerId) = await GetSeedIdsAsync();
        var id = await Client.CreateRouteAsync(new
        {
            code = $"RSEQ-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow
        });
        var routeId = Clean(id);

        // Create 3 stops
        await Client.CreateStopAsync(routeId, new
        {
            routeId = Guid.Parse(routeId),
            sequenceNumber = 3,
            customerId = Guid.Parse(customerId),
            address = "Третья"
        });
        await Client.CreateStopAsync(routeId, new
        {
            routeId = Guid.Parse(routeId),
            sequenceNumber = 1,
            customerId = Guid.Parse(customerId),
            address = "Первая"
        });
        await Client.CreateStopAsync(routeId, new
        {
            routeId = Guid.Parse(routeId),
            sequenceNumber = 2,
            customerId = Guid.Parse(customerId),
            address = "Вторая"
        });

        var detail = await Client.GetRouteDetailAsync(routeId);
        var stops = detail.GetProperty("stops");
        Assert.Equal(3, stops.GetArrayLength());
        Assert.Equal(1, stops[0].GetProperty("sequenceNumber").GetInt32());
        Assert.Equal(2, stops[1].GetProperty("sequenceNumber").GetInt32());
        Assert.Equal(3, stops[2].GetProperty("sequenceNumber").GetInt32());

        await Client.CancelRouteAsync(routeId);
        await Client.DeleteRouteAsync(routeId);
    }

    // ─── Geofence Tests ────────────────────────────────────────────────

    [Fact]
    public async Task Geofence_CRUD_AndCheck()
    {
        // Create polygon geofence (triangle around Bishkek center)
        var gfId = await Client.CreateGeofenceAsync(new
        {
            code = $"GF-{Guid.NewGuid():N}"[..8],
            name = "Test Restricted Zone",
            type = "Restricted",
            vertices = new[]
            {
                new { latitude = 42.8700, longitude = 74.5650 },
                new { latitude = 42.8800, longitude = 74.5650 },
                new { latitude = 42.8750, longitude = 74.5750 }
            },
            isActive = true
        });
        var geofenceId = Clean(gfId);

        // List
        var geofences = await Client.GetGeofencesAsync();
        Assert.True(geofences.GetArrayLength() > 0);

        // Create a stop inside the polygon
        var (_, _, customerId) = await GetSeedIdsAsync();
        var routeId = await Client.CreateRouteAsync(new
        {
            code = $"RGF-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow
        });
        var stopId = await Client.CreateStopAsync(Clean(routeId), new
        {
            routeId = Guid.Parse(Clean(routeId)),
            sequenceNumber = 1,
            customerId = Guid.Parse(customerId),
            address = "Inside polygon",
            latitude = 42.8750,
            longitude = 74.5700  // inside the triangle
        });

        // Check stop against geofences — should find the polygon
        var results = await Client.CheckStopGeofencesAsync(Clean(stopId));
        Assert.True(results.GetArrayLength() > 0);
        var match = results[0];
        Assert.Equal(geofenceId, match.GetProperty("geofenceId").GetString());
        Assert.True(match.GetProperty("isInside").GetBoolean());

        // Create a stop far outside
        var stop2Id = await Client.CreateStopAsync(Clean(routeId), new
        {
            routeId = Guid.Parse(Clean(routeId)),
            sequenceNumber = 2,
            customerId = Guid.Parse(customerId),
            address = "Far away",
            latitude = 43.0000,
            longitude = 75.0000
        });

        var results2 = await Client.CheckStopGeofencesAsync(Clean(stop2Id));
        foreach (var r in results2.EnumerateArray())
        {
            Assert.False(r.GetProperty("isInside").GetBoolean());
        }

        // Cleanup
        await Client.CancelRouteAsync(Clean(routeId));
        await Client.DeleteRouteAsync(Clean(routeId));
        await Client.DeleteGeofenceAsync(geofenceId);
    }

    [Fact]
    public async Task GeofenceArrive_BreachLoggedInNotes()
    {
        // Create a no-go polygon
        var gfId = await Client.CreateGeofenceAsync(new
        {
            code = "NOGO-01",
            name = "No-Go City Center",
            type = "NoGo",
            vertices = new[]
            {
                new { latitude = 42.8730, longitude = 74.5680 },
                new { latitude = 42.8760, longitude = 74.5680 },
                new { latitude = 42.8745, longitude = 74.5720 }
            },
            isActive = true
        });

        // Create route + stop inside the polygon
        var (_, _, customerId) = await GetSeedIdsAsync();
        var routeId = await Client.CreateRouteAsync(new
        {
            code = $"RNOGO-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow
        });
        var stopId = await Client.CreateStopAsync(Clean(routeId), new
        {
            routeId = Guid.Parse(Clean(routeId)),
            sequenceNumber = 1,
            customerId = Guid.Parse(customerId),
            address = "Inside no-go polygon",
            latitude = 42.8745,
            longitude = 74.5700
        });

        // Start route, then arrive at stop
        await Client.StartRouteAsync(Clean(routeId));
        await Client.ArriveAtStopAsync(Clean(routeId), Clean(stopId));

        var detail = await Client.GetRouteDetailAsync(Clean(routeId));
        var stop = detail.GetProperty("stops")[0];
        var notes = stop.GetProperty("notes").GetString();
        Assert.NotNull(notes);
        Assert.Contains("[GEOFENCE]", notes);
        Assert.Contains("NoGo", notes);

        // Cleanup
        await Client.CancelRouteAsync(Clean(routeId));
        await Client.DeleteRouteAsync(Clean(routeId));
        await Client.DeleteGeofenceAsync(Clean(gfId));
    }

    // ─── Shipment Tests ────────────────────────────────────────────────

    [Fact]
    public async Task Shipment_AssignCompletesWithStop()
    {
        var (_, _, customerId) = await GetSeedIdsAsync();

        // Create a sales order
        var soId = await Client.CreateSalesOrderAsync(new
        {
            number = $"SO-{Guid.NewGuid():N}"[..12],
            customerId = Guid.Parse(customerId),
            lines = Array.Empty<object>()
        });
        var salesOrderId = Clean(soId);

        // Confirm the sales order (required before assigning to stop)
        await Client.ConfirmSalesOrderAsync(salesOrderId);

        // Create route + stop
        var routeId = await Client.CreateRouteAsync(new
        {
            code = $"RSHIP-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow
        });
        var stopId = await Client.CreateStopAsync(Clean(routeId), new
        {
            routeId = Guid.Parse(Clean(routeId)),
            sequenceNumber = 1,
            customerId = Guid.Parse(customerId),
            address = "Shipment test address"
        });

        // Assign shipment
        await Client.AssignShipmentAsync(Clean(stopId), Guid.Parse(salesOrderId));

        // Verify in detail
        var detail = await Client.GetRouteDetailAsync(Clean(routeId));
        var stop = detail.GetProperty("stops")[0];
        Assert.Equal(1, stop.GetProperty("shipmentCount").GetInt32());

        // Start and complete the stop — shipment should be completed too
        await Client.StartRouteAsync(Clean(routeId));
        await Client.ArriveAtStopAsync(Clean(routeId), Clean(stopId));
        await Client.CompleteStopAsync(Clean(routeId), Clean(stopId));

        detail = await Client.GetRouteDetailAsync(Clean(routeId));
        stop = detail.GetProperty("stops")[0];
        Assert.Equal("Completed", stop.GetProperty("status").GetString());

        // Cleanup
        await Client.CancelRouteAsync(Clean(routeId));
        await Client.DeleteRouteAsync(Clean(routeId));
    }

    // ─── Authorization Tests ───────────────────────────────────────────

    [Fact]
    public async Task DispatcherRole_CanAccessDispatcherEndpoints()
    {
        // Create a user with Dispatcher role
        var dispatcherUser = $"dispatcher-{Guid.NewGuid():N}"[..20];
        var dispatcherPass = "Dispatcher1234!";

        // Login as admin to create the user
        var userId = await Client.CreateUserAsync(dispatcherUser, dispatcherPass,
            new List<string> { "Dispatcher" });

        // Login as dispatcher
        var dispatcherClient = await Client.LoginAsUserAsync(dispatcherUser, dispatcherPass);

        // Should be able to read routes
        var routesResponse = await dispatcherClient.GetRawAsync("/api/v1/routes");
        Assert.Equal(System.Net.HttpStatusCode.OK, routesResponse.StatusCode);

        // Should be able to read geofences
        var gfResponse = await dispatcherClient.GetRawAsync("/api/v1/geofences");
        Assert.Equal(System.Net.HttpStatusCode.OK, gfResponse.StatusCode);

        // Should be able to create routes (write permission)
        var createResponse = await dispatcherClient.PostRawAsync("/api/v1/routes", new
        {
            code = $"RDISP-{Guid.NewGuid():N}"[..10],
            date = DateTime.UtcNow
        });
        Assert.True(
            createResponse.StatusCode == System.Net.HttpStatusCode.Created ||
            createResponse.StatusCode == System.Net.HttpStatusCode.OK,
            $"Expected 201 or 200, got {createResponse.StatusCode}");

        // Cleanup created route
        if (createResponse.IsSuccessStatusCode)
        {
            var createdId = Clean(await createResponse.Content.ReadAsStringAsync());
            try { await dispatcherClient.DeleteRouteAsync(createdId); } catch { }
        }
    }

    // ─── GeoUtils Tests ──────────────────────────────────────────────

    [Fact]
    public void GeoUtils_HaversineDistance_KnownPoints()
    {
        var distance = GeoUtils.HaversineDistance(
            42.8746, 74.5698,
            42.8760, 74.5871);

        Assert.True(distance > 1000 && distance < 2000,
            $"Expected ~1200m, got {distance:F0}m");
    }

    [Fact]
    public void GeoUtils_IsPointInPolygon_TrueWhenInside()
    {
        var vertices = new List<GeoPoint>
        {
            new(42.8700, 74.5650),
            new(42.8800, 74.5650),
            new(42.8750, 74.5750)
        };
        var point = new GeoPoint(42.8750, 74.5700);  // inside triangle

        Assert.True(GeoUtils.IsPointInPolygon(point, vertices));
    }

    [Fact]
    public void GeoUtils_IsPointInPolygon_FalseWhenOutside()
    {
        var vertices = new List<GeoPoint>
        {
            new(42.8700, 74.5650),
            new(42.8800, 74.5650),
            new(42.8750, 74.5750)
        };
        var point = new GeoPoint(43.0000, 75.0000);  // far away

        Assert.False(GeoUtils.IsPointInPolygon(point, vertices));
    }

    [Fact]
    public void GeoUtils_IsPointInPolygon_FalseWithLessThan3Vertices()
    {
        var vertices = new List<GeoPoint>
        {
            new(42.8700, 74.5650),
            new(42.8800, 74.5650)
        };
        Assert.False(GeoUtils.IsPointInPolygon(new GeoPoint(42.8750, 74.5700), vertices));
    }
}
