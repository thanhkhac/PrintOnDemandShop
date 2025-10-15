using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.ProductDesigns.Dtos;

namespace CleanArchitectureBase.Application.ProductDesigns.Queries;

[Authorize]
public class SearchProductDesignsQuery : IRequest<PaginatedList<ProductDesignDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductOptionValueId { get; set; }
}

public class SearchProductDesignsQueryValidator : AbstractValidator<SearchProductDesignsQuery>
{
    public SearchProductDesignsQueryValidator()
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

public class SearchProductDesignsQueryHandler : IRequestHandler<SearchProductDesignsQuery, PaginatedList<ProductDesignDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public SearchProductDesignsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<PaginatedList<ProductDesignDto>> Handle(SearchProductDesignsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProductDesigns
            .Include(pd => pd.Product)
            .Include(pd => pd.ProductOptionValue)
            .ThenInclude(pov => pov!.ProductOption)
            .Include(pd => pd.Icons.Where(i => !i.IsDeleted))
            .Where(pd => pd.CreatedBy == _user.UserId && !pd.IsDeleted) // Only user's own non-deleted designs
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(pd => 
                (pd.Name != null && pd.Name.Contains(request.SearchTerm)) ||
                (pd.Product != null && pd.Product.Name != null && pd.Product.Name.Contains(request.SearchTerm)));
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(pd => pd.ProductId == request.ProductId.Value);
        }

        if (request.ProductOptionValueId.HasValue)
        {
            query = query.Where(pd => pd.ProductOptionValueId == request.ProductOptionValueId.Value);
        }

        // Order by creation date descending
        query = query.OrderByDescending(pd => pd.CreatedAt);

        var projectedQuery = query.Select(pd => new ProductDesignDto
        {
            Id = pd.Id,
            ProductId = pd.ProductId,
            ProductOptionValueId = pd.ProductOptionValueId,
            Name = pd.Name,
            CreatedAt = pd.CreatedAt,
            LastModifiedAt = pd.LastModifiedAt,
            ProductName = pd.Product != null ? pd.Product.Name : null,
            ProductOptionValue = pd.ProductOptionValue != null ? pd.ProductOptionValue.Value : null,
            Icons = pd.Icons.Select(i => new ProductDesignIconDto
            {
                Id = i.Id,
                ProductDesignId = i.ProductDesignId,
                ImageUrl = i.ImageUrl
            }).ToList(),
            Templates = new List<ProductDesignTemplateDto>() // Will be loaded separately if needed for performance
        });

        return await projectedQuery.PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
