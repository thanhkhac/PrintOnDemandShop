using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.IClients;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Products.Commands;

public class DeleteProductCommand : IRequest<bool>
{
    public Guid? ProductId { get; set; }
}

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");
    }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private List<Guid> _deleteProductVariantIds = new();
    private readonly IAiClient _aiClient;

    public DeleteProductCommandHandler(IApplicationDbContext context, IAiClient aiClient)
    {
        _context = context;
        _aiClient = aiClient;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Options)
            .ThenInclude(o => o.Values)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);

        if (product == null)
            throw new ErrorCodeException(ErrorCodes.PRODUCT_NOT_FOUND);
        //
        // // Check if product is being used in active orders
        // var hasActiveOrders = await _context.OrderItems
        //     .AnyAsync(oi => oi.ProductVariant!.ProductId == request.ProductId 
        //                     && oi.Order!.Status != "CANCELLED" 
        //                     && oi.Order.Status != "COMPLETED", cancellationToken);
        //
        // if (hasActiveOrders)
        //     throw new ErrorCodeException(ErrorCodes.PRODUCT_IN_USE);

        // Soft delete product and related entities
        product.IsDeleted = true;

        // Soft delete all variants
        foreach (var variant in product.Variants)
        {
            variant.IsDeleted = true;
            _deleteProductVariantIds.Add(variant.Id);
        }

        // Soft delete all option values
        foreach (var option in product.Options)
        {
            foreach (var value in option.Values)
            {
                value.IsDeleted = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        
        try
        {               
            await _aiClient.DeleteProductVariant(new
            {
                product_variant_ids = _deleteProductVariantIds
            });
        }
        catch (Exception)
        {
        }

        return true;
    }
}
