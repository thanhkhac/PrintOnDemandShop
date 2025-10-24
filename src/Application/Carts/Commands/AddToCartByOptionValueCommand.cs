using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Carts.Commands;

[Authorize]
public class AddToCartByOptionValueCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }

    // Danh sách OptionValueIds (ví dụ [Color=Red, Size=M])
    public List<Guid> ProductOptionValueIds { get; set; } = new();

    public Guid? ProductDesignId { get; set; }
    public int Quantity { get; set; }
}

public class AddToCartByOptionValueCommandValidator : AbstractValidator<AddToCartByOptionValueCommand>
{
    public AddToCartByOptionValueCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(x => x.ProductOptionValueIds)
            .NotEmpty()
            .WithMessage("At least one ProductOptionValueId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Quantity cannot exceed 100");
    }
}

public class AddToCartByOptionValueCommandHandler : IRequestHandler<AddToCartByOptionValueCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public AddToCartByOptionValueCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(AddToCartByOptionValueCommand request, CancellationToken cancellationToken)
    {
        // Validate product exists
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);

        if (product == null)
            throw new ErrorCodeException(ErrorCodes.PRODUCT_NOT_FOUND);

        // 🔍 Tìm ProductVariant tương ứng với OptionValueIds được chọn
        var targetOptionValueIds = request.ProductOptionValueIds;

        var productVariant = await _context.ProductVariants
            .Include(pv => pv.VariantValues)
            .ThenInclude(pvv => pvv.ProductOptionValue)
            .Where(pv => pv.ProductId == request.ProductId && !pv.IsDeleted)
            .FirstOrDefaultAsync(pv =>
                pv.VariantValues.Count(vv => targetOptionValueIds.Contains(vv.ProductOptionValueId)) == targetOptionValueIds.Count
                && pv.VariantValues.Count == targetOptionValueIds.Count,
                cancellationToken);

        if (productVariant == null)
            throw new ErrorCodeException(ErrorCodes.PRODUCT_VARIANT_NOT_FOUND);

        // 🧮 Kiểm tra tồn kho
        if (productVariant.Stock < request.Quantity)
            throw new ErrorCodeException(ErrorCodes.INSUFFICIENT_STOCK);

        // 🔍 Validate ProductDesign nếu có
        if (request.ProductDesignId.HasValue)
        {
            var productDesign = await _context.ProductDesigns
                .FirstOrDefaultAsync(pd => pd.Id == request.ProductDesignId.Value && !pd.IsDeleted, cancellationToken);

            if (productDesign == null)
                throw new ErrorCodeException(ErrorCodes.PRODUCT_DESIGN_NOT_FOUND);
        }

        // 🔁 Kiểm tra sản phẩm đã có trong giỏ hàng chưa
        var existingCartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci =>
                ci.CreatedBy == _user.UserId &&
                ci.ProductVariantId == productVariant.Id &&
                ci.ProductDesignId == request.ProductDesignId,
                cancellationToken);

        if (existingCartItem != null)
        {
            var newQuantity = existingCartItem.Quantity + request.Quantity;

            if (newQuantity > productVariant.Stock)
                throw new ErrorCodeException(ErrorCodes.INSUFFICIENT_STOCK);

            existingCartItem.Quantity = newQuantity;
            await _context.SaveChangesAsync(cancellationToken);

            return existingCartItem.Id;
        }

        // ➕ Thêm mới
        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            ProductVariantId = productVariant.Id,
            ProductDesignId = request.ProductDesignId,
            ProductId = productVariant.ProductId,
            Quantity = request.Quantity
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync(cancellationToken);

        return cartItem.Id;
    }
}
