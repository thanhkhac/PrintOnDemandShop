using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Templates.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class DeleteTemplateCommand : IRequest<bool>
{
    public Guid TemplateId { get; set; }
}

public class DeleteTemplateCommandValidator : AbstractValidator<DeleteTemplateCommand>
{
    public DeleteTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("TemplateId is required");
    }
}

public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && !t.IsDeleted, cancellationToken);

        if (template == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "Template not found");

        // Soft delete template
        template.IsDeleted = true;

        // Lấy tất cả ProductDesignTemplate liên quan
        var designTemplates = await _context.ProductDesignTemplates
            .Where(dt => dt.TemplateId == template.Id && !dt.IsDeleted)
            .ToListAsync(cancellationToken);

        var designIds = designTemplates.Select(dt => dt.ProductDesignId).Distinct().ToList();

        // Soft delete ProductDesignTemplate
        foreach (var dt in designTemplates)
        {
            dt.IsDeleted = true;
        }

        // Soft delete ProductDesign
        var designs = await _context.ProductDesigns
            .Where(pd => designIds.Contains(pd.Id) && !pd.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var pd in designs)
        {
            pd.IsDeleted = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
