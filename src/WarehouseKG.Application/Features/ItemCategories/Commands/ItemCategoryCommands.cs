using MediatR;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Domain.Entities;

namespace WarehouseKG.Application.Features.ItemCategories.Commands;

public record CreateItemCategoryCommand(
    string Code,
    string Name,
    string? Description = null) : IRequest<Guid>;

public class CreateItemCategoryCommandHandler : IRequestHandler<CreateItemCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateItemCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateItemCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ItemCategory
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description
        };

        _context.ItemCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}

public record UpdateItemCategoryCommand(
    Guid Id,
    string Code,
    string Name,
    string? Description) : IRequest<bool>;

public class UpdateItemCategoryCommandHandler : IRequestHandler<UpdateItemCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateItemCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateItemCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ItemCategories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (category is null)
            return false;

        category.Code = request.Code;
        category.Name = request.Name;
        category.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteItemCategoryCommand(Guid Id) : IRequest<bool>;

public class DeleteItemCategoryCommandHandler : IRequestHandler<DeleteItemCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteItemCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteItemCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ItemCategories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (category is null)
            return false;

        _context.ItemCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
