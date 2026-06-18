using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.StockReceipts.Commands;
using WarehouseKG.Application.Features.StockReceipts.Dtos;
using WarehouseKG.Application.Features.StockReceipts.Queries;
using WarehouseKG.Domain.Enums;
using WarehouseKG.Domain.Identity;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages stock receipts (receiving) for the current tenant.
/// </summary>
[Route("api/v1/stock-receipts")]
public class StockReceiptsController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly IApplicationDbContext _context;

    public StockReceiptsController(ISender sender, IApplicationDbContext context)
    {
        _sender = sender;
        _context = context;
    }

    /// <summary>Returns all stock receipts.</summary>
    [HttpGet]
    [Authorize(Policy = "stock-receipts:read")]
    [ProducesResponseType(typeof(IReadOnlyList<StockReceiptSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockReceiptSummaryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetStockReceiptsQuery(), cancellationToken));

    /// <summary>Returns a single stock receipt by id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "stock-receipts:read")]
    [ProducesResponseType(typeof(StockReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockReceiptDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockReceiptByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a draft stock receipt.</summary>
    [HttpPost]
    [Authorize(Policy = "stock-receipts:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateStockReceiptRequest body,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenant_id")!.Value);
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        var command = new CreateStockReceiptCommand(
            body.Number, body.WarehouseId, body.SupplierReference,
            body.Notes, body.ReceivedAtUtc, tenantId, roles, body.Lines);
        try
        {
            var id = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Completes a draft receipt and increases stock on hand.</summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "stock-receipts:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CompleteStockReceiptCommand(id), cancellationToken));

    /// <summary>Cancels a draft receipt.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "stock-receipts:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelStockReceiptCommand(id), cancellationToken));

    /// <summary>
    /// Deletes a stock receipt. Draft receipts require stock-receipts:delete.
    /// Completed receipts additionally require the stock-receipts-delete-completed scenario permission.
    /// Reverses stock quantities if the receipt was completed.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "stock-receipts:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        // Check if the receipt is completed
        var receipt = await _context.StockReceipts
            .AsNoTracking()
            .Select(r => new { r.Id, r.Status })
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (receipt is null) return NotFound();

        // Completed receipts require the stock-receipts-delete-completed scenario permission
        if (receipt.Status == StockOperationStatus.Completed)
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")!.Value);
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var hasPerm = await _context.TenantPermissions
                .AnyAsync(p =>
                    p.TenantId == tenantId &&
                    roles.Contains(p.RoleName) &&
                    p.Resource == Resources.ReceivingDelete &&
                    p.CanDelete,
                    cancellationToken);

            if (!hasPerm)
                return Forbid();
        }

        return MapWorkflowResult(await _sender.Send(new DeleteStockReceiptCommand(id), cancellationToken));
    }
}

/// <summary>Request body for creating a stock receipt.</summary>
public record CreateStockReceiptRequest(
    string Number,
    Guid WarehouseId,
    string? SupplierReference,
    string? Notes,
    DateTime? ReceivedAtUtc,
    IReadOnlyList<StockReceiptLineInput> Lines);
