using System.ComponentModel;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Templates.Dtos;

namespace CleanArchitectureBase.Application.Templates.Queries;

public class SearchTemplatesQuery : IRequest<PaginatedList<TemplateDto>>
{
    [DefaultValue(1)] 
    public int PageNumber { get; set; } = 1;
    [DefaultValue(10)] 
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductOptionValueId { get; set; }
}

public class SearchTemplatesQueryValidator : AbstractValidator<SearchTemplatesQuery>
{
    public SearchTemplatesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}

public class SearchTemplatesQueryHandler : IRequestHandler<SearchTemplatesQuery, PaginatedList<TemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TemplateDto>> Handle(SearchTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Templates
            .Include(t => t.Product)
            .Include(t => t.ProductOptionValue)
            .ThenInclude(pov => pov!.ProductOption)
            .Where(t => !t.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(t => 
                (t.PrintAreaName != null && t.PrintAreaName.Contains(request.SearchTerm)) ||
                (t.Product != null && t.Product.Name != null && t.Product.Name.Contains(request.SearchTerm)) ||
                (t.ProductOptionValue != null && t.ProductOptionValue.Value != null && t.ProductOptionValue.Value.Contains(request.SearchTerm)));
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(t => t.ProductId == request.ProductId.Value);
        }

        if (request.ProductOptionValueId.HasValue)
        {
            query = query.Where(t => t.ProductOptionValueId == request.ProductOptionValueId.Value);
        }

        // Order by creation date descending
        query = query.OrderByDescending(t => t.CreatedAt);

        var projectedQuery = query.Select(t => new TemplateDto
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
        });

        return await projectedQuery.PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
