using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Templates.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class CreateOrUpdateTemplateCommand : IRequest<Guid>
{
    public Guid? TemplateId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductOptionValueId { get; set; }
    public string PrintAreaName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class CreateOrUpdateTemplateCommandValidator : AbstractValidator<CreateOrUpdateTemplateCommand>
{
    public CreateOrUpdateTemplateCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(x => x.ProductOptionValueId)
            .NotEmpty()
            .WithMessage("ProductOptionValueId is required");

        RuleFor(x => x.PrintAreaName)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("PrintAreaName is required and must not exceed 200 characters");

        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("ImageUrl is required and must not exceed 2000 characters");
    }
}

public class CreateOrUpdateTemplateCommandHandler : IRequestHandler<CreateOrUpdateTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateOrUpdateTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrUpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        // Validate Product exists
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);

        if (!productExists)
            throw new ErrorCodeException(ErrorCodes.PRODUCT_NOT_FOUND);

        // Validate ProductOptionValue exists
        var productOptionValue = await _context.ProductOptionValues
            .Include(pov => pov.ProductOption)
            .FirstOrDefaultAsync(pov => pov.Id == request.ProductOptionValueId && !pov.IsDeleted, cancellationToken);

        if (productOptionValue == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "ProductOptionValue not found");

        // Validate ProductOptionValue belongs to the Product
        if (productOptionValue.ProductOption?.ProductId != request.ProductId)
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_REQUEST, "ProductOptionValue does not belong to the specified Product");

        Template? template;
        bool imageUpdated = false;
        if (request.TemplateId.HasValue)
        {
            // Update existing template
            template = await _context.Templates
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId.Value, cancellationToken);

            if (template == null)
                throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "Template not found");

            if (template.ImageUrl != request.ImageUrl)
            {
                imageUpdated = true;
            }


            template.ProductId = request.ProductId;
            template.ProductOptionValueId = request.ProductOptionValueId;
            template.PrintAreaName = request.PrintAreaName;
            template.ImageUrl = request.ImageUrl;
        }
        else
        {
            // Create new template
            template = new Template
            {
                ProductId = request.ProductId,
                ProductOptionValueId = request.ProductOptionValueId,
                PrintAreaName = request.PrintAreaName,
                ImageUrl = request.ImageUrl
            };

            _context.Templates.Add(template);
        }


        if (imageUpdated)
        {
            // Lấy tất cả các ProductDesignTemplate liên quan
            var designTemplates = await _context.ProductDesignTemplates
                .Where(dt => dt.TemplateId == template.Id && !dt.IsDeleted)
                .ToListAsync(cancellationToken);

            // Lấy danh sách ProductDesign Id liên quan
            var designIds = designTemplates.Select(dt => dt.ProductDesignId).Distinct().ToList();

            // Soft delete tất cả ProductDesignTemplate
            foreach (var dt in designTemplates)
            {
                dt.IsDeleted = true;
            }

            // Lấy tất cả ProductDesign liên quan
            var designs = await _context.ProductDesigns
                .Where(pd => designIds.Contains(pd.Id) && !pd.IsDeleted)
                .ToListAsync(cancellationToken);

            // Soft delete ProductDesign
            foreach (var pd in designs)
            {
                pd.IsDeleted = true;
            }

        }


        await _context.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
