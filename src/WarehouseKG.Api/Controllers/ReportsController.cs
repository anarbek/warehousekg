using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    [ProducesResponseType(typeof(InventorySummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventorySummaryReportDto>> GetInventorySummary(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetInventorySummaryReportQuery(), cancellationToken));

    /// <summary>Returns active items at or below their reorder level, most deficient first.</summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IReadOnlyList<LowStockItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LowStockItemDto>>> GetLowStock(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetLowStockReportQuery(), cancellationToken));

    /// <summary>Returns sales order counts and value, broken down by status.</summary>
    [HttpGet("sales-summary")]
    [ProducesResponseType(typeof(SalesSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalesSummaryReportDto>> GetSalesSummary(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetSalesSummaryReportQuery(), cancellationToken));

    /// <summary>Returns purchase order counts and value, broken down by status.</summary>
    [HttpGet("purchase-summary")]
    [ProducesResponseType(typeof(PurchaseSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseSummaryReportDto>> GetPurchaseSummary(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPurchaseSummaryReportQuery(), cancellationToken));

    /// <summary>Returns counts of each stock operation grouped by Draft / Completed / Cancelled.</summary>
    [HttpGet("stock-movements")]
    [ProducesResponseType(typeof(StockMovementSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StockMovementSummaryReportDto>> GetStockMovements(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetStockMovementSummaryReportQuery(), cancellationToken));
}
