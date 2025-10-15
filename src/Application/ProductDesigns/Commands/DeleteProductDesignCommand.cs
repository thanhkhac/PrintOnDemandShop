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
            .FirstOrDefaultAsync(pd => pd.Id == request.ProductDesignId && pd.CreatedBy == _user.UserId, cancellationToken);

        if (productDesign == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "ProductDesign not found or you don't have permission to delete it");

        // Check if design is being used in any cart items or orders
        var isDesignInUse = await _context.CartItems
            .AnyAsync(ci => ci.ProductDesignId == request.ProductDesignId, cancellationToken);

        if (!isDesignInUse)
        {
            isDesignInUse = await _context.OrderItems
                .AnyAsync(oi => oi.ProductDesignId == request.ProductDesignId, cancellationToken);
        }

        if (isDesignInUse)
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_REQUEST, "Cannot delete design as it is being used in cart or orders");

        // Remove related entities
        var designTemplates = await _context.ProductDesignTemplates
            .Where(pdt => pdt.ProductDesignId == request.ProductDesignId)
            .ToListAsync(cancellationToken);

        _context.ProductDesignTemplates.RemoveRange(designTemplates);
        _context.ProductDesignIcons.RemoveRange(productDesign.Icons);
        _context.ProductDesigns.Remove(productDesign);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
