using System.Linq.Dynamic.Core;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Products.Dtos;
using CleanArchitectureBase.Application.Products.Dtos.Response;

namespace CleanArchitectureBase.Application.Products.Queries;

public class SearchProductQuery : PaginatedQuery, IRequest<PaginatedList<ProductForSearchResponseDto>>
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public long? MinPrice { get; set; }
    public long? MaxPrice { get; set; }
    public string? SortBy { get; set; } = "NAME";
    public bool SortDescending { get; set; } = false;
}

public class SearchProductQueryValidator : PaginatedQueryValidator<SearchProductQuery>
{
    public SearchProductQueryValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice");

        RuleFor(x => x.SortBy)
            .Must(x => new[]
            {
                "NAME", "PRICE", "CREATED_ON"
            }.Contains(x))
            .WithMessage("SortBy must be one of: NAME, PRICE, CREATED_ON");
    }
}

public class SearchProductQueryHandler : IRequestHandler<SearchProductQuery, PaginatedList<ProductForSearchResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchProductQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProductForSearchResponseDto>> Handle(SearchProductQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.CreatedByUser)
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Search by name or description
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name!.Contains(searchTerm) ||
                p.Description!.Contains(searchTerm));
        }

        // Filter by category
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.ProductCategories
                .Any(pc => pc.CategoryId == request.CategoryId.Value));
        }

        // Calculate price range from variants or use base price
        var queryWithPrice = query.Select(p => new
        {
            Product = p,
            MinPrice = p.Variants.Any(v => !v.IsDeleted)
                ? p.Variants.Where(v => !v.IsDeleted).Min(v => v.UnitPrice)
                : p.BasePrice,
            MaxPrice = p.Variants.Any(v => !v.IsDeleted)
                ? p.Variants.Where(v => !v.IsDeleted).Max(v => v.UnitPrice)
                : p.BasePrice
        });

        // Filter by price range
        if (request.MinPrice.HasValue)
        {
            queryWithPrice = queryWithPrice.Where(x => x.MaxPrice >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            queryWithPrice = queryWithPrice.Where(x => x.MinPrice <= request.MaxPrice.Value);
        }

        // Apply sorting
        var sortExpression = request.SortBy switch
        {
            "NAME" => request.SortDescending ? "Product.Name DESC" : "Product.Name",
            "PRICE" => request.SortDescending ? "MinPrice DESC" : "MinPrice",
            "CREATED_ON" => request.SortDescending ? "Product.CreatedAt DESC" : "Product.CreatedAt",
            _ => "Product.Name"
        };

        queryWithPrice = queryWithPrice.OrderBy(sortExpression);

        // Select final result
        var finalQuery = queryWithPrice.AsQueryable().Select(x => new ProductForSearchResponseDto
        {
            ProductId = x.Product.Id,
            Name = x.Product.Name,
            Description = x.Product.Description,
            ImageUrl = x.Product.ImageUrl,
            MinPrice = x.MinPrice,  
            MaxPrice = x.MaxPrice, 
            CreatedBy = new CreatedByDto
            {
                UserId = x.Product.CreatedBy,
                Name = x.Product.CreatedBy.HasValue ? "Admin" : null
            }
        });

        return await PaginatedList<ProductForSearchResponseDto>
            .CreateAsync(finalQuery, request.PageNumber, request.PageSize);
    }
}
