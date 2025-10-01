using CleanArchitectureBase.Application.Categories.Dtos;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Categories.Queries;


public class GetCategoryByIdQuery : IRequest<CategoryDto>
{
    public Guid Id { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var allCategories = await _context.Categories
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var category = allCategories.FirstOrDefault(c => c.Id == request.Id);
        if (category == null)
            throw new ErrorCodeException(ErrorCodes.CATEGORY_NOT_FOUND);

        return BuildCategoryDto(category, allCategories);
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
