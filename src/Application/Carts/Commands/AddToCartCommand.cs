using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Carts.Commands;

[Authorize]
public class AddToCartCommand : IRequest<Guid>
{
    public Guid? ProductVariantId { get; set; }
    public Guid? ProductDesignId { get; set; }
    public int Quantity { get; set; }
}

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty()
            .WithMessage("ProductVariantId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Quantity cannot exceed 100");
    }
}

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public AddToCartCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Validate product variant exists and is available
        var productVariant = await _context.ProductVariants
            .Include(pv => pv.Product)
            .FirstOrDefaultAsync(pv => pv.Id == request.ProductVariantId 
                && !pv.IsDeleted 
                && !pv.Product!.IsDeleted, cancellationToken);

        if (productVariant == null)
            throw new ErrorCodeException(ErrorCodes.PRODUCT_VARIANT_NOT_FOUND);

        // Check stock availability
        if (productVariant.Stock < request.Quantity)
            throw new ErrorCodeException(ErrorCodes.INSUFFICIENT_STOCK);

        // Validate product design if provided
        if (request.ProductDesignId.HasValue)
        {
            var productDesign = await _context.ProductDesigns
                .FirstOrDefaultAsync(pd => pd.Id == request.ProductDesignId.Value, cancellationToken);

            if (productDesign == null)
                throw new ErrorCodeException(ErrorCodes.PRODUCT_DESIGN_NOT_FOUND);
        }

        // Check if item already exists in cart
        var existingCartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CreatedBy == _user.UserId
                && ci.ProductVariantId == request.ProductVariantId
                && ci.ProductDesignId == request.ProductDesignId, cancellationToken);

        if (existingCartItem != null)
        {
            // Update quantity if item already exists
            var newQuantity =  request.Quantity;
            
            if (newQuantity > productVariant.Stock)
                throw new ErrorCodeException(ErrorCodes.INSUFFICIENT_STOCK);

            existingCartItem.Quantity = newQuantity;
            await _context.SaveChangesAsync(cancellationToken);
            
            return existingCartItem.Id;
        }

        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            ProductVariantId = request.ProductVariantId!.Value,
            ProductDesignId = request.ProductDesignId,
            ProductId = productVariant.ProductId,
            Quantity = request.Quantity
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync(cancellationToken);

        return cartItem.Id;
    }
}
