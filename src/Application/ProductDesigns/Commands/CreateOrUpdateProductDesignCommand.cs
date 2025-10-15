using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.ProductDesigns.Commands;

[Authorize]
public class CreateOrUpdateProductDesignCommand : IRequest<Guid>
{
    public Guid? ProductDesignId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductOptionValueId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public List<CreateProductDesignIconRequest> Icons { get; set; } = new();
    public List<CreateProductDesignTemplateRequest> Templates { get; set; } = new();
}

public class CreateProductDesignIconRequest
{
    public string ImageUrl { get; set; } = string.Empty;
}

public class CreateProductDesignTemplateRequest
{
    public Guid TemplateId { get; set; }
    public string? DesignImageUrl { get; set; }
    // Removed PrintAreaName since it's automatically taken from Template
}

public class CreateOrUpdateProductDesignCommandValidator : AbstractValidator<CreateOrUpdateProductDesignCommand>
{
    public CreateOrUpdateProductDesignCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(x => x.ProductOptionValueId)
            .NotEmpty()
            .WithMessage("ProductOptionValueId is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Name is required and must not exceed 500 characters");

        RuleForEach(x => x.Icons)
            .SetValidator(new CreateProductDesignIconValidator());

        RuleForEach(x => x.Templates)
            .SetValidator(new CreateProductDesignTemplateValidator());
    }
}

public class CreateProductDesignIconValidator : AbstractValidator<CreateProductDesignIconRequest>
{
    public CreateProductDesignIconValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("ImageUrl is required and must not exceed 2000 characters");
    }
}

public class CreateProductDesignTemplateValidator : AbstractValidator<CreateProductDesignTemplateRequest>
{
    public CreateProductDesignTemplateValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("TemplateId is required");

        RuleFor(x => x.DesignImageUrl)
            .MaximumLength(2000)
            .WithMessage("DesignImageUrl must not exceed 2000 characters");
        
        // Removed PrintAreaName validation since it's a snapshot
    }
}

public class CreateOrUpdateProductDesignCommandHandler : IRequestHandler<CreateOrUpdateProductDesignCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateOrUpdateProductDesignCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(CreateOrUpdateProductDesignCommand request, CancellationToken cancellationToken)
    {
        // Validate Product exists
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);
        
        if (!productExists)
            throw new ErrorCodeException(ErrorCodes.PRODUCT_NOT_FOUND);

        // Validate ProductOptionValue exists and belongs to the Product
        var productOptionValue = await _context.ProductOptionValues
            .Include(pov => pov.ProductOption)
            .FirstOrDefaultAsync(pov => pov.Id == request.ProductOptionValueId && !pov.IsDeleted, cancellationToken);
        
        if (productOptionValue == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "ProductOptionValue not found");

        if (productOptionValue.ProductOption?.ProductId != request.ProductId)
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_REQUEST, "ProductOptionValue does not belong to the specified Product");

        // Validate Templates exist and get their data for snapshots
        var templateIds = request.Templates.Select(t => t.TemplateId).ToList();
        var templates = new Dictionary<Guid, Template>();
        
        if (templateIds.Any())
        {
            var existingTemplates = await _context.Templates
                .Where(t => templateIds.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync(cancellationToken);
                
            if (existingTemplates.Count != templateIds.Count)
                throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "One or more templates not found");
                
            templates = existingTemplates.ToDictionary(t => t.Id, t => t);
        }

        ProductDesign? productDesign;

        if (request.ProductDesignId.HasValue)
        {
            // Update existing design
            productDesign = await _context.ProductDesigns
                .Include(pd => pd.Icons)
                .FirstOrDefaultAsync(pd => pd.Id == request.ProductDesignId.Value && pd.CreatedBy == _user.UserId, cancellationToken);

            if (productDesign == null)
                throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "ProductDesign not found or you don't have permission to edit it");

            productDesign.ProductId = request.ProductId;
            productDesign.ProductOptionValueId = request.ProductOptionValueId;
            productDesign.Name = request.Name;

            // Remove existing icons
            _context.ProductDesignIcons.RemoveRange(productDesign.Icons);

            // Remove existing templates
            var existingTemplates = await _context.ProductDesignTemplates
                .Where(pdt => pdt.ProductDesignId == productDesign.Id)
                .ToListAsync(cancellationToken);
            _context.ProductDesignTemplates.RemoveRange(existingTemplates);
        }
        else
        {
            // Create new design
            productDesign = new ProductDesign
            {
                ProductId = request.ProductId,
                ProductOptionValueId = request.ProductOptionValueId,
                Name = request.Name
            };

            _context.ProductDesigns.Add(productDesign);
        }

        // Add icons
        foreach (var iconRequest in request.Icons)
        {
            var icon = new ProductDesignIcons
            {
                ProductDesignId = productDesign.Id,
                ImageUrl = iconRequest.ImageUrl
            };
            _context.ProductDesignIcons.Add(icon);
        }

        // Add templates with PrintAreaName snapshot from Template
        foreach (var templateRequest in request.Templates)
        {
            var template = templates[templateRequest.TemplateId];
            var designTemplate = new ProductDesignTemplate
            {
                ProductDesignId = productDesign.Id,
                TemplateId = templateRequest.TemplateId,
                DesignImageUrl = templateRequest.DesignImageUrl,
                PrintAreaName = template.PrintAreaName // Snapshot from Template
            };
            _context.ProductDesignTemplates.Add(designTemplate);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return productDesign.Id;
    }
}
