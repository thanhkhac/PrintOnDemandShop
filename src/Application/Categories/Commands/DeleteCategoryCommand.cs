using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Categories.Commands;

public class DeleteCategoryCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var allCategories = await _context.Categories
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var rootCategory = allCategories.FirstOrDefault(c => c.Id == request.Id);
        if (rootCategory == null)
            throw new ErrorCodeException(ErrorCodes.CATEGORY_NOT_FOUND);

        SoftDeleteCategoryRecursive(rootCategory, allCategories);

        await _context.SaveChangesAsync(cancellationToken);
    }


    private void SoftDeleteCategoryRecursive(Category? category, List<Category> all)
    {
        if (category == null) return;

        category.IsDeleted = true;

        var subCategories = all
            .Where(c => c.ParentCategoryId == category.Id && !c.IsDeleted)
            .ToList();

        foreach (var sub in subCategories)
        {
            SoftDeleteCategoryRecursive(sub, all);
        }
    }
}
