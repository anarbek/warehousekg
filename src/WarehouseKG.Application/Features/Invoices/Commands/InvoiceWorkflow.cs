using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Enums;

namespace WarehouseKG.Application.Features.Invoices.Commands;

public record IssueInvoiceCommand(Guid Id) : IRequest<OperationResult>;

public class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public IssueInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice is null) return OperationResult.NotFound;
        if (invoice.Status != InvoiceStatus.Draft) return OperationResult.InvalidState;

        invoice.Status = InvoiceStatus.Issued;
        invoice.IssuedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}

public record PrintInvoiceCommand(Guid Id) : IRequest<OperationResult>;

public class PrintInvoiceCommandHandler : IRequestHandler<PrintInvoiceCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public PrintInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(PrintInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice is null) return OperationResult.NotFound;
        if (invoice.Status is not InvoiceStatus.Issued and not InvoiceStatus.Printed)
            return OperationResult.InvalidState;

        invoice.Status = InvoiceStatus.Printed;
        invoice.PrintedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}

public record SignInvoiceCommand(Guid Id, string? SignedByName = null) : IRequest<OperationResult>;

public class SignInvoiceCommandHandler : IRequestHandler<SignInvoiceCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public SignInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(SignInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice is null) return OperationResult.NotFound;
        if (invoice.Status is not InvoiceStatus.Issued and not InvoiceStatus.Printed)
            return OperationResult.InvalidState;

        invoice.Status = InvoiceStatus.Signed;
        invoice.SignedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.SignedByName))
            invoice.SignedByName = request.SignedByName;

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}

public record CancelInvoiceCommand(Guid Id) : IRequest<OperationResult>;

public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public CancelInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice is null) return OperationResult.NotFound;
        if (invoice.Status is InvoiceStatus.Signed or InvoiceStatus.Cancelled)
            return OperationResult.InvalidState;

        invoice.Status = InvoiceStatus.Cancelled;

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}

public record DeleteInvoiceCommand(Guid Id) : IRequest<OperationResult>;

public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, OperationResult>
{
    private readonly IApplicationDbContext _context;

    public DeleteInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<OperationResult> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice is null) return OperationResult.NotFound;

        _context.InvoiceLines.RemoveRange(invoice.Lines);
        _context.Invoices.Remove(invoice);

        await _context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success;
    }
}
