using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Invoices.Commands;

public record InvoiceLineInput(
    Guid InventoryItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate = 0m,
    string? Notes = null);

public record CreateInvoiceCommand(
    Guid? SalesOrderId,
    Guid CustomerId,
    Guid WarehouseId,
    string Currency,
    decimal ExchangeRate,
    string? PaymentType,
    DateTime? DueDateUtc,
    string? Notes,
    IReadOnlyList<InvoiceLineInput> Lines) : IRequest<Guid>;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateInvoiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Generate sequential invoice number
        var number = await GenerateInvoiceNumberAsync(cancellationToken);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Number = number,
            Type = InvoiceType.Sales,
            Status = InvoiceStatus.Draft,
            SalesOrderId = request.SalesOrderId,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "KGS" : request.Currency,
            ExchangeRate = request.ExchangeRate > 0 ? request.ExchangeRate : 1m,
            PaymentType = request.PaymentType,
            DueDateUtc = request.DueDateUtc,
            Notes = request.Notes,
            IssuedAtUtc = DateTime.UtcNow,
            Lines = request.Lines.Select(l => new InvoiceLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.Quantity * l.UnitPrice,
                TaxRate = l.TaxRate,
                TaxAmount = l.Quantity * l.UnitPrice * l.TaxRate / 100m,
                Notes = l.Notes
            }).ToList()
        };

        invoice.TotalAmount = invoice.Lines.Sum(l => l.LineTotal);
        invoice.TaxAmount = invoice.Lines.Sum(l => l.TaxAmount);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var lastInvoice = await _context.Invoices
            .Where(i => i.Number.StartsWith(prefix))
            .OrderByDescending(i => i.Number)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastInvoice is null)
            return $"{prefix}0001";

        var lastNumber = lastInvoice.Number[prefix.Length..];
        if (int.TryParse(lastNumber, out var num))
            return $"{prefix}{(num + 1):D4}";

        return $"{prefix}0001";
    }
}

public record UpdateInvoiceCommand(
    Guid Id,
    Guid? SalesOrderId,
    Guid CustomerId,
    Guid WarehouseId,
    string Currency,
    decimal ExchangeRate,
    string? PaymentType,
    DateTime? DueDateUtc,
    string? Notes,
    IReadOnlyList<InvoiceLineInput> Lines) : IRequest<OperationResult>;

public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public UpdateInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(UpdateInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.Id, ct);

        if (invoice is null) return OperationResult.NotFound;
        if (invoice.Status != InvoiceStatus.Draft) return OperationResult.InvalidState;

        invoice.SalesOrderId = request.SalesOrderId;
        invoice.CustomerId = request.CustomerId;
        invoice.WarehouseId = request.WarehouseId;
        invoice.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "KGS" : request.Currency;
        invoice.ExchangeRate = request.ExchangeRate > 0 ? request.ExchangeRate : 1m;
        invoice.PaymentType = request.PaymentType;
        invoice.DueDateUtc = request.DueDateUtc;
        invoice.Notes = request.Notes;

        // Query existing lines separately (without Include to avoid tracking conflicts)
        var existingLines = await _context.InvoiceLines
            .Where(l => l.InvoiceId == request.Id)
            .ToListAsync(ct);

        var existingByItemId = existingLines.ToDictionary(l => l.InventoryItemId);
        var requestItemIds = request.Lines.Select(l => l.InventoryItemId).ToHashSet();

        // Remove lines not in request
        foreach (var (itemId, line) in existingByItemId)
        {
            if (!requestItemIds.Contains(itemId))
            {
                _context.InvoiceLines.Remove(line);
            }
        }

        await _context.SaveChangesAsync(ct);

        // Update existing and add new lines (in a separate SaveChanges to avoid tracking conflicts)
        foreach (var l in request.Lines)
        {
            if (existingByItemId.TryGetValue(l.InventoryItemId, out var existing))
            {
                existing.Quantity = l.Quantity;
                existing.UnitPrice = l.UnitPrice;
                existing.LineTotal = l.Quantity * l.UnitPrice;
                existing.TaxRate = l.TaxRate;
                existing.TaxAmount = l.Quantity * l.UnitPrice * l.TaxRate / 100m;
                existing.Notes = l.Notes;
            }
            else
            {
                _context.InvoiceLines.Add(new InvoiceLine
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = l.InventoryItemId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.Quantity * l.UnitPrice,
                    TaxRate = l.TaxRate,
                    TaxAmount = l.Quantity * l.UnitPrice * l.TaxRate / 100m,
                    Notes = l.Notes,
                    InvoiceId = invoice.Id
                });
            }
        }

        invoice.TotalAmount = request.Lines.Sum(l => l.Quantity * l.UnitPrice);
        invoice.TaxAmount = request.Lines.Sum(l => l.Quantity * l.UnitPrice * l.TaxRate / 100m);

        await _context.SaveChangesAsync(ct);
        return OperationResult.Success;
    }
}
