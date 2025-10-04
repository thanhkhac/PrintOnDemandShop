using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Products.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Products.Commands;

public class CreateProductCommand : IRequest<Guid>
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long BasePrice { get; set; }
    public Guid CategoryId { get; set; }

    public List<ProductOptionRqDto> Options { get; set; } = new();
    public List<ProductVariantRqDto> Variants { get; set; } = new();
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
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

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            BasePrice = request.BasePrice,
            IsDeleted = false
        };

        product.ProductCategories.Add(new ProductCategory
        {
            ProductId = product.Id,
            CategoryId = request.CategoryId
        });

        foreach (var optionDto in request.Options)
        {
            var newProductOption = new ProductOption
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Name = optionDto.Name
            };

            foreach (var valueDto in optionDto.Values)
            {
                var newPov = new ProductOptionValue
                {
                    Id = Guid.NewGuid(),
                    ProductOptionId = newProductOption.Id,
                    Value = valueDto,
                };
                newProductOption.Values.Add(newPov);
            }
        }

        foreach (var variantDto in request.Variants)
        {
            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                UnitPrice = variantDto.Price,
                Stock = variantDto.Stock,
                Sku = variantDto.Sku,
                IsDeleted = false
            };
            product.Variants.Add(variant);
        }
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
