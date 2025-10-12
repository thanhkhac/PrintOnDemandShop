using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Products.Dtos;
using CleanArchitectureBase.Application.Products.Dtos.RequestDtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Products.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class CreateUpdateProductCommand : IRequest<Guid>
{
    /// <summary>
    /// ProductId
    /// </summary>
    public Guid? ProductId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long BasePrice { get; set; }
    public Guid CategoryId { get; set; }

    public List<UpsertProductOptionRequest> Options { get; set; } = new();
    public List<UpsertProductVariantRequestDto> Variants { get; set; } = new();
}

public class CreateProductCommandValidator : AbstractValidator<CreateUpdateProductCommand>
{
    private static readonly string[] AllowedOptions =
    {
        "SIZE", "COLOR"
    };

    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.BasePrice)
            .GreaterThan(0);

        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Options)
            .NotEmpty();

        RuleFor(x => x.Options)
            .NotEmpty()
            .WithMessage("Product must have options.")
            .Must(options => options.All(o => AllowedOptions.Contains(o.Name)))
            .WithMessage("Only 'SIZE' and 'COLOR' are allowed as options.")
            .Must(options => options.Select(o => o.Name).Distinct().Count() == options.Count)
            .WithMessage("Duplicate options are not allowed.")
            .Must(options => options.Count <= 2)
            .WithMessage("Only 'Size' and 'Color' options are allowed, maximum 2.");
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateUpdateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateUpdateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra category có tồn tại không
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            throw new ErrorCodeException(ErrorCodes.CATEGORY_NOT_FOUND);

        Product? product;
        if (request.ProductId.HasValue)
        {
            // @formatter:off
            product = await _context.Products
                .Include(p => p.Options)
                    .ThenInclude(o => o.Values.Where(v => !v.IsDeleted))
                    .ThenInclude(v => v.Images.Where(i => !i.IsDeleted))
                .Include(p => p.Variants.Where(v => !v.IsDeleted))
                    .ThenInclude(v => v.VariantValues)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId.Value && !p.IsDeleted, cancellationToken);
            // @formatter:on
            if (product == null)
                throw new ErrorCodeException(ErrorCodes.PRODUCT_NOT_FOUND);

            // Update các thuộc tính chung
            product.Name = request.Name;
            product.Description = request.Description;
            product.ImageUrl = request.ImageUrl;
            product.BasePrice = request.BasePrice;

            var currentCategory = product.ProductCategories.FirstOrDefault();
            if (currentCategory?.CategoryId != request.CategoryId)
            {
                _context.ProductCategories.RemoveRange(product.ProductCategories);
                product.ProductCategories.Add(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = request.CategoryId
                });
            }


            // Update options và values
            UpdateProductOptions(product, request.Options, cancellationToken);
            //
            // // Update variants
            UpdateProductVariants(product, request.Variants, cancellationToken);
        }
        else
        {
            // Tạo sản phẩm mới
            product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                BasePrice = request.BasePrice,
                IsDeleted = false
            };
            
            //Gán category cho nó
            product.ProductCategories.Add(new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = request.CategoryId
            });

            foreach (var optionRequest in request.Options)
            {
                var option = new ProductOption
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = optionRequest.Name
                };

                foreach (var valueRequest in optionRequest.Values)
                {
                    var optionValue = new ProductOptionValue
                    {
                        Id = Guid.NewGuid(),
                        ProductOptionId = option.Id,
                        Value = valueRequest.Value,
                        IsDeleted = false
                    };

                    // Add images for option values
                    for (int i = 0; i < valueRequest.ImageUrl.Count; i++)
                    {
                        var image = new ProductOptionValueImage
                        {
                            Id = Guid.NewGuid(),
                            ProductOptionValueId = optionValue.Id,
                            ImageUrl = valueRequest.ImageUrl[i],
                            Order = i,
                            IsDeleted = false
                        };
                        optionValue.Images.Add(image);
                    }

                    option.Values.Add(optionValue);
                }

                product.Options.Add(option);
            }


            foreach (var variantRequest in request.Variants)
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Sku = variantRequest.Sku,
                    UnitPrice = variantRequest.Price,
                    Stock = variantRequest.Stock,
                    IsDeleted = false
                };


                foreach (var optionValuePair in variantRequest.OptionValues)
                {
                    var optionName = optionValuePair.Key;
                    var optionValueText = optionValuePair.Value;

                    var optionValue = product.Options
                        .Where(o => o.Name == optionName)
                        .SelectMany(o => o.Values)
                        .FirstOrDefault(v => v.Value == optionValueText);

                    if (optionValue != null)
                    {
                        variant.VariantValues.Add(new ProductVariantValue
                        {
                            ProductVariantId = variant.Id,
                            ProductOptionValueId = optionValue.Id
                        });
                    }
                }

                product.Variants.Add(variant);
            }

            _context.Products.Add(product);
        }


        await _context.SaveChangesAsync(cancellationToken);
        return product.Id;
    }


    private void UpdateOptionValues(ProductOption option, List<UpsertOptionValueRequest> valueRequests, CancellationToken cancellationToken)
    {
        var requestValueIds = valueRequests.Where(v => v.OptionValueId.HasValue).Select(v => v.OptionValueId!.Value).ToList();

        // Xóa values không còn trong request
        var valuesToDelete = option.Values.Where(v => !requestValueIds.Contains(v.Id)).ToList();
        valuesToDelete.ForEach(x => x.IsDeleted = true);

        foreach (var valueRequest in valueRequests)
        {
            if (valueRequest.OptionValueId.HasValue)
            {
                // Update existing value
                var existingValue = option.Values.FirstOrDefault(v => v.Id == valueRequest.OptionValueId.Value);
                if (existingValue != null)
                {
                    existingValue.Value = valueRequest.Value;

                    existingValue.Images.ForEach(i => i.IsDeleted = true);
                    for (int i = 0; i < valueRequest.ImageUrl.Count; i++)
                    {    
                        //TODO: Tìm hiểu tại sao code cũ lỗi
                        //Code cũ existingValue.Images.Add(new ProductOptionValueImage 
                        _context.ProductOptionValueImages.Add(new ProductOptionValueImage
                        {
                            Id = Guid.NewGuid(),
                            ProductOptionValueId = existingValue.Id,
                            ImageUrl = valueRequest.ImageUrl[i],
                            Order = i,
                            IsDeleted = false
                        });
                    }
                }
            }
            else
            {
                // Create new value
                var newValue = new ProductOptionValue
                {
                    Id = Guid.NewGuid(),
                    ProductOptionId = option.Id,
                    Value = valueRequest.Value,
                    IsDeleted = false
                };

                for (int i = 0; i < valueRequest.ImageUrl.Count; i++)
                {
                    newValue.Images.Add(new ProductOptionValueImage
                    {
                        Id = Guid.NewGuid(),
                        ProductOptionValueId = newValue.Id,
                        ImageUrl = valueRequest.ImageUrl[i],
                        Order = i,
                        IsDeleted = false
                    });
                }

                option.Values.Add(newValue);
            }
        }
    }

    //Cập nhật các option
    private void UpdateProductOptions(Product product, List<UpsertProductOptionRequest> optionRequests, CancellationToken cancellationToken)
    {
        foreach (var optionRequest in optionRequests)
        {
            ProductOption? option;

            if (optionRequest.OptionId.HasValue)
            {
                // Update existing option
                option = product.Options.FirstOrDefault(o => o.Id == optionRequest.OptionId.Value);
                if (option != null)
                {
                    option.Name = optionRequest.Name;
                    UpdateOptionValues(option, optionRequest.Values, cancellationToken);
                }
            }
        }
    }


    private void UpdateProductVariants(Product product, List<UpsertProductVariantRequestDto> variantRequests, CancellationToken cancellationToken)
    {
        // Lấy tất cả variantId
        var requestVariantIds = variantRequests.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToList();

        // Xóa variants không còn trong request
        var variantsToSoftDelete = product.Variants.Where(v => !requestVariantIds.Contains(v.Id)).ToList();
        foreach (var variantToDelete in variantsToSoftDelete)
        {
            variantToDelete.IsDeleted = true;
        }

        // Xử lý variant
        foreach (var variantRequest in variantRequests)
        {
            ProductVariant? variant;

            if (variantRequest.Id.HasValue)
            {
                // Update existing variant
                variant = product.Variants.FirstOrDefault(v => v.Id == variantRequest.Id.Value);
                if (variant != null)
                {
                    variant.Sku = variantRequest.Sku;
                    variant.UnitPrice = variantRequest.Price;
                    variant.Stock = variantRequest.Stock;
                }
            }
            else
            {
                // Create new variant
                variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Sku = variantRequest.Sku,
                    UnitPrice = variantRequest.Price,
                    Stock = variantRequest.Stock,
                    IsDeleted = false
                };

                // Link variant with option values
                foreach (var optionValuePair in variantRequest.OptionValues)
                {
                    var optionName = optionValuePair.Key;
                    var optionValueText = optionValuePair.Value;

                    var optionValue = product.Options
                        .Where(o => o.Name == optionName)
                        .SelectMany(o => o.Values)
                        .FirstOrDefault(v => v.Value == optionValueText);

                    if (optionValue != null)
                    {
                        variant.VariantValues.Add(new ProductVariantValue
                        {
                            ProductVariantId = variant.Id,
                            ProductOptionValueId = optionValue.Id
                        });
                    }
                }

                product.Variants.Add(variant);
            }
        }
    }
}
