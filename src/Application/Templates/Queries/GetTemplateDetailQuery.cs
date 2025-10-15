using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Templates.Dtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Templates.Queries;

public class GetTemplateDetailQuery : IRequest<TemplateDetailDto>
{
    public Guid TemplateId { get; set; }
}

public class GetTemplateDetailQueryValidator : AbstractValidator<GetTemplateDetailQuery>
{
    public GetTemplateDetailQueryValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("TemplateId is required");
    }
}

public class GetTemplateDetailQueryHandler : IRequestHandler<GetTemplateDetailQuery, TemplateDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetTemplateDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateDetailDto> Handle(GetTemplateDetailQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .Include(t => t.Product)
            .Include(t => t.ProductOptionValue)
            .ThenInclude(pov => pov!.ProductOption)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "Template not found");

        return new TemplateDetailDto
        {
            Id = template.Id,
            ProductId = template.ProductId,
            ProductOptionValueId = template.ProductOptionValueId,
            PrintAreaName = template.PrintAreaName,
            ImageUrl = template.ImageUrl,
            CreatedAt = template.CreatedAt,
            LastModifiedAt = template.LastModifiedAt,
            ProductName = template.Product?.Name,
            ProductOptionName = template.ProductOptionValue?.ProductOption?.Name,
            ProductOptionValue = template.ProductOptionValue?.Value,
            Product = template.Product != null ? new ProductDto
            {
                Id = template.Product.Id,
                Name = template.Product.Name,
                Description = template.Product.Description,
                ImageUrl = template.Product.ImageUrl
            } : null,
            ProductOptionValueDetail = template.ProductOptionValue != null ? new ProductOptionValueDto
            {
                Id = template.ProductOptionValue.Id,
                Value = template.ProductOptionValue.Value,
                OptionName = template.ProductOptionValue.ProductOption?.Name
            } : null
        };
    }
}
