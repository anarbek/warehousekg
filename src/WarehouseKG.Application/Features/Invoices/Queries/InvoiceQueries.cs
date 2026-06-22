using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.Invoices.Dtos;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Invoices.Queries;

public record GetInvoicesQuery(
    InvoiceStatus? Status = null,
    Guid? CustomerId = null,
    Guid? WarehouseId = null,
    Guid? SalesOrderId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<IReadOnlyList<InvoiceSummaryDto>>;

public class GetInvoicesQueryHandler
    : IRequestHandler<GetInvoicesQuery, IReadOnlyList<InvoiceSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoicesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<InvoiceSummaryDto>> Handle(
        GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(i => i.CustomerId == request.CustomerId.Value);

        if (request.WarehouseId.HasValue)
            query = query.Where(i => i.WarehouseId == request.WarehouseId.Value);

        if (request.SalesOrderId.HasValue)
            query = query.Where(i => i.SalesOrderId == request.SalesOrderId.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(i => i.IssuedAtUtc >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(i => i.IssuedAtUtc <= request.DateTo.Value);

        return await query
            .OrderByDescending(i => i.IssuedAtUtc)
            .ThenBy(i => i.Number)
            .Select(i => new InvoiceSummaryDto
            {
                Id = i.Id,
                Number = i.Number,
                Type = i.Type,
                Status = i.Status,
                CustomerId = i.CustomerId,
                CustomerName = i.Customer != null ? i.Customer.Name : null,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse != null ? i.Warehouse.Name : null,
                IssuedAtUtc = i.IssuedAtUtc,
                CreatedAt = i.CreatedAt,
                TotalAmount = i.TotalAmount,
                TaxAmount = i.TaxAmount,
                Currency = i.Currency,
                SalesOrderId = i.SalesOrderId,
                SalesOrderNumber = i.SalesOrder != null ? i.SalesOrder.Number : null,
                LineCount = i.Lines.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto?>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly IApplicationDbContext _context;

    public GetInvoiceByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<InvoiceDto?> Handle(
        GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Id == request.Id)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                Number = i.Number,
                Type = i.Type,
                Status = i.Status,
                SalesOrderId = i.SalesOrderId,
                SalesOrderNumber = i.SalesOrder != null ? i.SalesOrder.Number : null,
                PurchaseOrderId = i.PurchaseOrderId,
                CustomerId = i.CustomerId,
                CustomerName = i.Customer != null ? i.Customer.Name : null,
                SupplierId = i.SupplierId,
                SupplierName = i.Supplier != null ? i.Supplier.Name : null,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse != null ? i.Warehouse.Name : null,
                IssuedAtUtc = i.IssuedAtUtc,
                PrintedAtUtc = i.PrintedAtUtc,
                SignedAtUtc = i.SignedAtUtc,
                DueDateUtc = i.DueDateUtc,
                TotalAmount = i.TotalAmount,
                TaxAmount = i.TaxAmount,
                Currency = i.Currency,
                ExchangeRate = i.ExchangeRate,
                PaymentType = i.PaymentType,
                PrintedBy = i.PrintedBy,
                SignedByName = i.SignedByName,
                SignatureDataUrl = i.SignatureDataUrl,
                Notes = i.Notes,
                ExternalReference = i.ExternalReference,
                CreatedAt = i.CreatedAt,
                Lines = i.Lines.Select(l => new InvoiceLineDto
                {
                    Id = l.Id,
                    InventoryItemId = l.InventoryItemId,
                    InventoryItemName = l.InventoryItem != null ? l.InventoryItem.Sku : null,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal,
                    TaxRate = l.TaxRate,
                    TaxAmount = l.TaxAmount,
                    Notes = l.Notes
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
