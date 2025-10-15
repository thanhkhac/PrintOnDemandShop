using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Templates.Dtos;

namespace CleanArchitectureBase.Application.Templates.Queries;

public class GetTemplatesByProductQuery : IRequest<List<TemplateDto>>
{
    public Guid ProductId { get; set; }
    public Guid? ProductOptionValueId { get; set; }
}

public class GetTemplatesByProductQueryValidator : AbstractValidator<GetTemplatesByProductQuery>
{
    public GetTemplatesByProductQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");
    }
}    

public class GetTemplatesByProductQueryHandler : IRequestHandler<GetTemplatesByProductQuery, List<TemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTemplatesByProductQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TemplateDto>> Handle(GetTemplatesByProductQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Templates
            .Include(t => t.Product)
            .Include(t => t.ProductOptionValue)
            .ThenInclude(pov => pov!.ProductOption)
            .Where(t => t.ProductId == request.ProductId && !t.IsDeleted);

        if (request.ProductOptionValueId.HasValue)
        {
            query = query.Where(t => t.ProductOptionValueId == request.ProductOptionValueId.Value);
        }

        var templates = await query
            .OrderBy(t => t.PrintAreaName)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new TemplateDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductOptionValueId = t.ProductOptionValueId,
                PrintAreaName = t.PrintAreaName,
                ImageUrl = t.ImageUrl,
                CreatedAt = t.CreatedAt,
                LastModifiedAt = t.LastModifiedAt,
                ProductName = t.Product != null ? t.Product.Name : null,
                ProductOptionName = t.ProductOptionValue != null && t.ProductOptionValue.ProductOption != null ? t.ProductOptionValue.ProductOption.Name : null,
                ProductOptionValue = t.ProductOptionValue != null ? t.ProductOptionValue.Value : null
            })
            .ToListAsync(cancellationToken);

        return templates;
    }
}
