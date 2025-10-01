using CleanArchitectureBase.Application.Categories.Dtos;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Categories.Queries;

public class GetAllCategoriesQuery : IRequest<List<CategoryDto>> { }

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var allCategories = await _context.Categories
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        // Build tree: chỉ lấy các root categories (ParentCategoryId == null)
        var rootCategories = allCategories
            .Where(c => c.ParentCategoryId == null)
            .Select(c => BuildCategoryDto(c, allCategories))
            .ToList();

        return rootCategories;
    }

    private CategoryDto BuildCategoryDto(Category category, List<Category> allCategories)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            SubCategories = allCategories
                .Where(c => c.ParentCategoryId == category.Id)
                .Select(c => BuildCategoryDto(c, allCategories))
                .ToList()
        };
    }
}
