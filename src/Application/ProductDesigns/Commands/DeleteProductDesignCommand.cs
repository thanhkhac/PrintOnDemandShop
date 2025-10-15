using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.ProductDesigns.Commands;

[Authorize]
public class DeleteProductDesignCommand : IRequest<bool>
{
    public Guid ProductDesignId { get; set; }
}

public class DeleteProductDesignCommandValidator : AbstractValidator<DeleteProductDesignCommand>
{
    public DeleteProductDesignCommandValidator()
    {
        RuleFor(x => x.ProductDesignId)
            .NotEmpty()
            .WithMessage("ProductDesignId is required");
    }
}

public class DeleteProductDesignCommandHandler : IRequestHandler<DeleteProductDesignCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteProductDesignCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<bool> Handle(DeleteProductDesignCommand request, CancellationToken cancellationToken)
    {
        var productDesign = await _context.ProductDesigns
            .Include(pd => pd.Icons)
            .FirstOrDefaultAsync(pd => pd.Id == request.ProductDesignId && pd.CreatedBy == _user.UserId && !pd.IsDeleted, cancellationToken);

        if (productDesign == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "ProductDesign not found or you don't have permission to delete it");

        // Soft delete related entities
        var designTemplates = await _context.ProductDesignTemplates
            .Where(pdt => pdt.ProductDesignId == request.ProductDesignId && !pdt.IsDeleted)
            .ToListAsync(cancellationToken);

        // Soft delete ProductDesign
        productDesign.IsDeleted = true;

        // Soft delete all related icons
        foreach (var icon in productDesign.Icons.Where(i => !i.IsDeleted))
        {
            icon.IsDeleted = true;
        }

        // Soft delete all related templates
        foreach (var template in designTemplates)
        {
            template.IsDeleted = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
