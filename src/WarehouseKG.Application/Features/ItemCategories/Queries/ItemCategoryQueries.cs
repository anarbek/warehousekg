using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseKG.Application.Common.Interfaces;
using WarehouseKG.Application.Features.ItemCategories.Dtos;

namespace WarehouseKG.Application.Features.ItemCategories.Queries;

public record GetItemCategoriesQuery : IRequest<IReadOnlyList<ItemCategoryDto>>;

public class GetItemCategoriesQueryHandler
    : IRequestHandler<GetItemCategoriesQuery, IReadOnlyList<ItemCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetItemCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ItemCategoryDto>> Handle(
        GetItemCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.ItemCategories
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new ItemCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                RequiresAgeVerification = c.RequiresAgeVerification
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetItemCategoryByIdQuery(Guid Id) : IRequest<ItemCategoryDto?>;

public class GetItemCategoryByIdQueryHandler
    : IRequestHandler<GetItemCategoryByIdQuery, ItemCategoryDto?>
{
    private readonly IApplicationDbContext _context;

    public GetItemCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ItemCategoryDto?> Handle(
        GetItemCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.ItemCategories
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new ItemCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                RequiresAgeVerification = c.RequiresAgeVerification
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
