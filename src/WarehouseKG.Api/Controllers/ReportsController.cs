using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.Reports.Dtos;
using WarehouseKG.Application.Features.Reports.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Read-only aggregate reports across inventory, orders, and stock operations for the current tenant.
/// </summary>
[Route("api/v1/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns aggregate inventory KPIs (totals, items below reorder, items out of stock).</summary>
    [HttpGet("inventory-summary")]
    [Authorize(Policy = "reports:read")]
    [ProducesResponseType(typeof(InventorySummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventorySummaryReportDto>> GetInventorySummary(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetInventorySummaryReportQuery(), cancellationToken));

    /// <summary>Returns active items at or below their reorder level, most deficient first.</summary>
    [HttpGet("low-stock")]
    [Authorize(Policy = "reports:read")]
    [ProducesResponseType(typeof(IReadOnlyList<LowStockItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LowStockItemDto>>> GetLowStock(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetLowStockReportQuery(), cancellationToken));

    /// <summary>Returns sales order counts and value, broken down by status.</summary>
    [HttpGet("sales-summary")]
    [Authorize(Policy = "reports:read")]
    [ProducesResponseType(typeof(SalesSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalesSummaryReportDto>> GetSalesSummary(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetSalesSummaryReportQuery(), cancellationToken));

    /// <summary>Returns purchase order counts and value, broken down by status.</summary>
    [HttpGet("purchase-summary")]
    [Authorize(Policy = "reports:read")]
    [ProducesResponseType(typeof(PurchaseSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseSummaryReportDto>> GetPurchaseSummary(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPurchaseSummaryReportQuery(), cancellationToken));

    /// <summary>Returns counts of each stock operation grouped by Draft / Completed / Cancelled.</summary>
    [HttpGet("stock-movements")]
    [Authorize(Policy = "reports:read")]
    [ProducesResponseType(typeof(StockMovementSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StockMovementSummaryReportDto>> GetStockMovements(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetStockMovementSummaryReportQuery(), cancellationToken));

    /// <summary>
    /// Calculates per-item stock at a warehouse by replaying all completed operations
    /// within an optional date range.
    /// </summary>
    [HttpGet("warehouse-stock")]
    [Authorize(Policy = "warehouses:read")]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseStockItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseStockItemDto>>> GetWarehouseStock(
        [FromQuery] Guid warehouseId,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
        => Ok(await _sender.Send(new GetWarehouseStockReportQuery(warehouseId, dateFrom, dateTo), cancellationToken));

    /// <summary>
    /// Returns chronological movement history for a single item at a warehouse,
    /// with running balance after each operation.
    /// </summary>
    [HttpGet("item-movements")]
    [Authorize(Policy = "inventory-items:read")]
    [ProducesResponseType(typeof(IReadOnlyList<ItemMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ItemMovementDto>>> GetItemMovements(
        [FromQuery] Guid itemId,
        [FromQuery] Guid warehouseId,
        CancellationToken cancellationToken = default)
        => Ok(await _sender.Send(new GetItemMovementHistoryQuery(itemId, warehouseId), cancellationToken));

    /// <summary>
    /// One-time backfill: creates initial StockReceipts for items with QuantityOnHand &gt; 0
    /// that have no prior receiving operations. Specify a warehouseId to assign them to.
    /// </summary>
    [HttpPost("backfill-initial-stock")]
    [Authorize(Policy = "reports:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BackfillInitialStock(
        [FromQuery] Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new BackfillInitialStockCommand(warehouseId), cancellationToken);
        return Ok(new { itemsBackfilled = result });
    }

    /// <summary>Returns a printable delivery manifest for a route with stops, shipments and items.</summary>
    [HttpGet("delivery-manifest/{routeId:guid}")]
    [Authorize(Policy = "reports:read")]
    [ProducesResponseType(typeof(DeliveryManifestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryManifestDto>> GetDeliveryManifest(Guid routeId, CancellationToken ct)
    {
        var r = await _sender.Send(new GetDeliveryManifestQuery(routeId), ct);
        return r is null ? NotFound() : Ok(r);
    }
}
