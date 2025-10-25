using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.ProductDesigns.Dtos;
using Microsoft.EntityFrameworkCore;

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
            .Where(pd => pd.CreatedBy == _user.UserId && !pd.IsDeleted)
            .OrderByDescending(pd => pd.CreatedAt)
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

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Paginate
        var productDesigns = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Get all design templates related to these designs
        var designIds = productDesigns.Select(pd => pd.Id).ToList();

        var designTemplates = await _context.ProductDesignTemplates
            .Include(pdt => pdt.Template)
            .Where(pdt => designIds.Contains(pdt.ProductDesignId) && !pdt.IsDeleted)
            .ToListAsync(cancellationToken);

        // Group templates by ProductDesignId
        var templateGroups = designTemplates
            .GroupBy(t => t.ProductDesignId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Map to DTOs
        var items = productDesigns.Select(pd => new ProductDesignDto
        {
            Id = pd.Id,
            ProductId = pd.ProductId,
            ProductOptionValueId = pd.ProductOptionValueId,
            Name = pd.Name,
            CreatedAt = pd.CreatedAt,
            LastModifiedAt = pd.LastModifiedAt,
            ProductName = pd.Product?.Name,
            ProductOptionValue = pd.ProductOptionValue?.Value,
            Icons = pd.Icons.Select(i => new ProductDesignIconDto
            {
                Id = i.Id,
                ProductDesignId = i.ProductDesignId,
                ImageUrl = i.ImageUrl
            }).ToList(),
            Templates = templateGroups.TryGetValue(pd.Id, out var templates)
                ? templates.Select(dt => new ProductDesignTemplateDto
                {
                    ProductDesignId = dt.ProductDesignId,
                    TemplateId = dt.TemplateId,
                    DesignImageUrl = dt.DesignImageUrl,
                    PrintAreaName = dt.PrintAreaName,
                    TemplateImageUrl = dt.Template?.ImageUrl
                }).ToList()
                : new List<ProductDesignTemplateDto>()
        }).ToList();

        return new PaginatedList<ProductDesignDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
