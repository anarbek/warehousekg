using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Features.Invoices.Commands;
using WarehouseKG.Application.Features.Invoices.Dtos;
using WarehouseKG.Application.Features.Invoices.Queries;
using WarehouseKG.Application.Features.Invoices.Queries;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Api.Controllers;

[Route("api/v1/invoices")]
public class InvoicesController : ApiControllerBase
{
    private readonly ISender _sender;
    private const string Resource = "invoices";

    public InvoicesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = Resource + ":read")]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InvoiceSummaryDto>>> GetAll(
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? salesOrderId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
        => Ok(await _sender.Send(new GetInvoicesQuery(status, customerId, warehouseId, salesOrderId, dateFrom, dateTo), cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Resource + ":read")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Resource + ":write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Resource + ":write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        return MapWorkflowResult(await _sender.Send(command, cancellationToken));
    }

    [HttpPost("{id:guid}/issue")]
    [Authorize(Policy = Resource + ":write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Issue(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new IssueInvoiceCommand(id), cancellationToken));

    [HttpPost("{id:guid}/print")]
    [Authorize(Policy = Resource + ":write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Print(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new PrintInvoiceCommand(id), cancellationToken));

    [HttpPost("{id:guid}/sign")]
    [Authorize(Policy = Resource + ":write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Sign(Guid id, [FromBody] SignInvoiceCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        return MapWorkflowResult(await _sender.Send(command, cancellationToken));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Resource + ":write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new CancelInvoiceCommand(id), cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Resource + ":delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => MapWorkflowResult(await _sender.Send(new DeleteInvoiceCommand(id), cancellationToken));

    [HttpGet("{id:guid}/pdf")]
    [Authorize(Policy = Resource + ":read")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _sender.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        if (invoice is null) return NotFound();

        var html = BuildInvoiceHtml(invoice);
        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
        return File(bytes, "text/html; charset=utf-8", $"Invoice-{invoice.Number}.html");
    }

    private static string BuildInvoiceHtml(InvoiceDto inv)
    {
        var totalQty = inv.Lines.Sum(l => l.Quantity);
        var grandTotal = inv.TotalAmount + inv.TaxAmount;
        var lines = string.Join("", inv.Lines.Select(l =>
            $"<tr><td>{System.Net.WebUtility.HtmlEncode(l.InventoryItemName ?? l.InventoryItemId.ToString())}</td>" +
            $"<td style='text-align:right'>{l.Quantity}</td>" +
            $"<td style='text-align:right'>{l.UnitPrice:N2}</td>" +
            $"<td style='text-align:right'>{l.LineTotal:N2}</td>" +
            $"<td style='text-align:right'>{l.TaxRate}%</td>" +
            $"<td style='text-align:right'>{l.TaxAmount:N2}</td>" +
            $"<td style='text-align:right'>{l.LineTotal + l.TaxAmount:N2}</td></tr>"));

        var sumsRow = $"<tfoot><tr style='background:#f5f5f5;font-weight:bold'>" +
            $"<td></td>" +
            $"<td style='text-align:right'>{totalQty}</td>" +
            $"<td></td>" +
            $"<td style='text-align:right'>{inv.TotalAmount:N2}</td>" +
            $"<td></td>" +
            $"<td style='text-align:right'>{inv.TaxAmount:N2}</td>" +
            $"<td style='text-align:right'>{grandTotal:N2}</td>" +
            $"</tr></tfoot>";

        return $@"<!DOCTYPE html><html><head><meta charset='utf-8'><title>Счёт {inv.Number}</title>
<style>body{{font-family:Arial,sans-serif;max-width:800px;margin:40px auto;color:#333}}
h1{{margin-bottom:0}}table{{width:100%;border-collapse:collapse;margin:20px 0}}
th,td{{border:1px solid #ddd;padding:8px;text-align:left}}th{{background:#f5f5f5}}
.total{{font-weight:bold;font-size:1.1em}}.header{{margin-bottom:20px}}
</style></head><body>
<h1>Счёт {System.Net.WebUtility.HtmlEncode(inv.Number)}</h1>
<div class='header'>
<p><strong>Клиент:</strong> {System.Net.WebUtility.HtmlEncode(inv.CustomerName ?? inv.CustomerId.ToString())}<br>
<strong>Склад:</strong> {System.Net.WebUtility.HtmlEncode(inv.WarehouseName ?? inv.WarehouseId.ToString())}<br>
<strong>Дата:</strong> {inv.IssuedAtUtc:dd.MM.yyyy}<br>
<strong>Валюта:</strong> {inv.Currency}</p>
</div>
<table><thead><tr><th>Товар</th><th>Кол-во</th><th>Цена</th><th>Сумма</th><th>Налог %</th><th>Сумма налога</th><th>Итого</th></tr></thead><tbody>{lines}</tbody>{sumsRow}</table>
<p class='total'>Сумма без налога: {inv.TotalAmount:N2} {inv.Currency}<br>
Налог: {inv.TaxAmount:N2} {inv.Currency}<br>
<strong>Итого: {inv.TotalAmount + inv.TaxAmount:N2} {inv.Currency}</strong></p>
{(string.IsNullOrWhiteSpace(inv.SignedByName) ? "" : $"<p><strong>Подписал:</strong> {System.Net.WebUtility.HtmlEncode(inv.SignedByName)}</p>")}
</body></html>";
    }
}
