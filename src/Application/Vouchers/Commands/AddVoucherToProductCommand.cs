using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Vouchers.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class AddVoucherToProductCommand : IRequest<bool>
{
    public Guid VoucherId { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
}

public class AddVoucherToProductCommandValidator : AbstractValidator<AddVoucherToProductCommand>
{
    public AddVoucherToProductCommandValidator()
    {
        RuleFor(x => x.VoucherId)
            .NotEmpty()
            .WithMessage("VoucherId is required");

        RuleFor(x => x.ProductIds)
            .NotEmpty()
            .WithMessage("At least one ProductId is required");

        RuleForEach(x => x.ProductIds)
            .NotEmpty()
            .WithMessage("ProductId cannot be empty");
    }
}

public class AddVoucherToProductCommandHandler : IRequestHandler<AddVoucherToProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AddVoucherToProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AddVoucherToProductCommand request, CancellationToken cancellationToken)
    {
        // Validate voucher exists
        var voucherExists = await _context.Vouchers
            .AnyAsync(v => v.Id == request.VoucherId, cancellationToken);

        if (!voucherExists)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "Voucher not found");

        // Validate all products exist
        var existingProductsCount = await _context.Products
            .CountAsync(p => request.ProductIds.Contains(p.Id) && !p.IsDeleted, cancellationToken);

        if (existingProductsCount != request.ProductIds.Count)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "One or more products not found");

        // Get existing associations to avoid duplicates
        var existingAssociations = await _context.ProductVouchers
            .Where(pv => pv.VoucherId == request.VoucherId && request.ProductIds.Contains(pv.ProductId))
            .Select(pv => pv.ProductId)
            .ToListAsync(cancellationToken);

        // Add new associations only for products that don't already have this voucher
        var newProductIds = request.ProductIds.Except(existingAssociations).ToList();

        foreach (var productId in newProductIds)
        {
            var productVoucher = new ProductVoucher
            {
                ProductId = productId,
                VoucherId = request.VoucherId
            };
            _context.ProductVouchers.Add(productVoucher);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
