using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.ProductDesigns.Dtos;

namespace CleanArchitectureBase.Application.ProductDesigns.Queries;

[Authorize]
public class GetProductDesignsByProductQuery : IRequest<List<ProductDesignDto>>
{
    public Guid ProductId { get; set; }
    public Guid? ProductOptionValueId { get; set; }
}

public class GetProductDesignsByProductQueryValidator : AbstractValidator<GetProductDesignsByProductQuery>
{
    public GetProductDesignsByProductQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");
    }
}

public class GetProductDesignsByProductQueryHandler : IRequestHandler<GetProductDesignsByProductQuery, List<ProductDesignDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetProductDesignsByProductQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<ProductDesignDto>> Handle(GetProductDesignsByProductQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProductDesigns
            .Include(pd => pd.Product)
            .Include(pd => pd.ProductOptionValue)
            .ThenInclude(pov => pov!.ProductOption)
            .Include(pd => pd.Icons)
            .Where(pd => pd.ProductId == request.ProductId && pd.CreatedBy == _user.UserId);

        if (request.ProductOptionValueId.HasValue)
        {
            query = query.Where(pd => pd.ProductOptionValueId == request.ProductOptionValueId.Value);
        }

        var productDesigns = await query
            .OrderBy(pd => pd.Name)
            .ThenBy(pd => pd.CreatedAt)
            .Select(pd => new ProductDesignDto
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
                Templates = new List<ProductDesignTemplateDto>() // Will be loaded separately if needed
            })
            .ToListAsync(cancellationToken);

        return productDesigns;
    }
}
