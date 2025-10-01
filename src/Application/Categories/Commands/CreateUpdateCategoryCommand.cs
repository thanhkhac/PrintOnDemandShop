using CleanArchitectureBase.Application.Categories.Dtos;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Categories.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class CreateUpdateCategoryCommand : IRequest<CategoryDto>
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public Guid? ParentCategoryId { get; set; }
}

public class CreateUpdateCategoryCommandValidator : AbstractValidator<CreateUpdateCategoryCommand>
{
    public CreateUpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Name is required");
    }
}

public class CreateUpdateCategoryCommandHandler : IRequestHandler<CreateUpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateUpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(CreateUpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        Category? category;

        if (request.Id != Guid.Empty && request.Id != null)
        {
            category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

            if (category == null) throw new ErrorCodeException(ErrorCodes.CATEGORY_NOT_FOUND);

            category.Name = request.Name;
        }
        else
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ParentCategoryId = request.ParentCategoryId
            };

            _context.Categories.Add(category);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            SubCategories = category.SubCategories
                .Where(sc => !sc.IsDeleted)
                .Select(sc => new CategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    ParentCategoryId = sc.ParentCategoryId
                }).ToList()
        };

        return dto;
    }
}
