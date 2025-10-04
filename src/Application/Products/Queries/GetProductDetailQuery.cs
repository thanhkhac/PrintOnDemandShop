using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Products.Dtos.ResponseDtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Products.Queries;

public class GetProductDetailQuery : IRequest<ProductDetailResponseDto>
{
    public Guid? ProductId { get; set; }
}

public class GetProductDetailQueryValidator : AbstractValidator<GetProductDetailQuery>
{
    public GetProductDetailQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");
    }
}

public class GetProductDetailQueryHandler : IRequestHandler<GetProductDetailQuery, ProductDetailResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailResponseDto> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .Include(p => p.Options)
            .ThenInclude(o => o.Values.Where(v => !v.IsDeleted))
            .ThenInclude(v => v.Images.Where(i => !i.IsDeleted))
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
            .ThenInclude(v => v.VariantValues)
            .ThenInclude(vv => vv.ProductOptionValue)
            .ThenInclude(pov => pov!.ProductOption)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);

        if (product == null)
            throw new ErrorCodeException(ErrorCodes.PRODUCT_NOT_FOUND);

        CategoryDto? categoryDto = null;
        if (product.ProductCategories.FirstOrDefault() is { } productCategory)
        {
            categoryDto = new CategoryDto
            {
                CategoryId = productCategory.CategoryId,
                Name = productCategory.Category!.Name
            };
        }

        return new ProductDetailResponseDto
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            BasePrice = product.BasePrice,
            Category = categoryDto,
            Options = product.Options.Select(o => new ProductOptionDto
            {
                OptionId = o.Id,
                Name = o.Name,
                Values = o.Values.Where(v => !v.IsDeleted).Select(v => new ProductOptionValueDto
                {
                    OptionValueId = v.Id,
                    Value = v.Value,
                    Images = v.Images.Where(i => !i.IsDeleted)
                        .OrderBy(i => i.Order)
                        .Select(i => i.ImageUrl!)
                        .ToList()
                }).ToList()
            }).ToList(),
            Variants = product.Variants.Where(v => !v.IsDeleted).Select(v => new ProductVariantDto
            {
                VariantId = v.Id,
                Sku = v.Sku,
                Price = v.UnitPrice,
                Stock = v.Stock,
                OptionValues = v.VariantValues.ToDictionary(
                    vv => vv.ProductOptionValue!.ProductOption!.Name!,
                    vv => vv.ProductOptionValue!.Value!
                )
            }).ToList()
        };
    }
}
