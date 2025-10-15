using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.ProductDesigns.Dtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.ProductDesigns.Queries;

[Authorize]
public class GetProductDesignDetailQuery : IRequest<ProductDesignDetailDto>
{
    public Guid ProductDesignId { get; set; }
}

public class GetProductDesignDetailQueryValidator : AbstractValidator<GetProductDesignDetailQuery>
{
    public GetProductDesignDetailQueryValidator()
    {
        RuleFor(x => x.ProductDesignId)
            .NotEmpty()
            .WithMessage("ProductDesignId is required");
    }
}

public class GetProductDesignDetailQueryHandler : IRequestHandler<GetProductDesignDetailQuery, ProductDesignDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetProductDesignDetailQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<ProductDesignDetailDto> Handle(GetProductDesignDetailQuery request, CancellationToken cancellationToken)
    {
        var productDesign = await _context.ProductDesigns
            .Include(pd => pd.Product)
            .Include(pd => pd.ProductOptionValue)
            .ThenInclude(pov => pov!.ProductOption)
            .Include(pd => pd.Icons)
            .FirstOrDefaultAsync(pd => pd.Id == request.ProductDesignId && pd.CreatedBy == _user.UserId, cancellationToken);

        if (productDesign == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "ProductDesign not found or you don't have permission to view it");

        // Get design templates
        var designTemplates = await _context.ProductDesignTemplates
            .Include(pdt => pdt.Template)
            .Where(pdt => pdt.ProductDesignId == request.ProductDesignId)
            .ToListAsync(cancellationToken);

        return new ProductDesignDetailDto
        {
            Id = productDesign.Id,
            ProductId = productDesign.ProductId,
            ProductOptionValueId = productDesign.ProductOptionValueId,
            Name = productDesign.Name,
            CreatedAt = productDesign.CreatedAt,
            LastModifiedAt = productDesign.LastModifiedAt,
            ProductName = productDesign.Product?.Name,
            ProductOptionValue = productDesign.ProductOptionValue?.Value,
            Product = productDesign.Product != null ? new ProductDto
            {
                Id = productDesign.Product.Id,
                Name = productDesign.Product.Name,
                Description = productDesign.Product.Description,
                ImageUrl = productDesign.Product.ImageUrl
            } : null,
            ProductOptionValueDetail = productDesign.ProductOptionValue != null ? new ProductOptionValueDto
            {
                Id = productDesign.ProductOptionValue.Id,
                Value = productDesign.ProductOptionValue.Value,
                OptionName = productDesign.ProductOptionValue.ProductOption?.Name
            } : null,
            Icons = productDesign.Icons.Select(i => new ProductDesignIconDto
            {
                Id = i.Id,
                ProductDesignId = i.ProductDesignId,
                ImageUrl = i.ImageUrl
            }).ToList(),
            Templates = designTemplates.Select(dt => new ProductDesignTemplateDto
            {
                ProductDesignId = dt.ProductDesignId,
                TemplateId = dt.TemplateId,
                DesignImageUrl = dt.DesignImageUrl,
                PrintAreaName = dt.PrintAreaName, // This is the snapshot
                TemplateImageUrl = dt.Template?.ImageUrl // From the actual Template
            }).ToList()
        };
    }
}
