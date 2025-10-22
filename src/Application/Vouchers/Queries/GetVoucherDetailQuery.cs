using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Vouchers.Dtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Vouchers.Queries;

// [Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class GetVoucherDetailQuery : IRequest<VoucherDetailDto>
{
    public Guid VoucherId { get; set; }
}

public class GetVoucherDetailQueryValidator : AbstractValidator<GetVoucherDetailQuery>
{
    public GetVoucherDetailQueryValidator()
    {
        RuleFor(x => x.VoucherId)
            .NotEmpty()
            .WithMessage("VoucherId is required");
    }
}

public class GetVoucherDetailQueryHandler : IRequestHandler<GetVoucherDetailQuery, VoucherDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetVoucherDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VoucherDetailDto> Handle(GetVoucherDetailQuery request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Id == request.VoucherId, cancellationToken);

        if (voucher == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "Voucher not found");

        // Get associated products
        var productVouchers = await _context.ProductVouchers
            .Include(pv => pv.Product)
            .Where(pv => pv.VoucherId == request.VoucherId && pv.Product != null && !pv.Product.IsDeleted)
            .ToListAsync(cancellationToken);

        return new VoucherDetailDto
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Description = voucher.Description,
            DiscountType = voucher.DiscountType,
            DiscountValue = voucher.DiscountValue,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            UsedCount = voucher.UsedCount,
            IsActive = voucher.IsActive,
            CreatedAt = voucher.CreatedAt,
            LastModifiedAt = voucher.LastModifiedAt,
            Products = productVouchers.Select(pv => new VoucherProductDto
            {
                ProductId = pv.ProductId,
                ProductName = pv.Product?.Name,
                ProductImageUrl = pv.Product?.ImageUrl
            }).ToList(),
            ProductDetails = productVouchers.Select(pv => new ProductDto
            {
                Id = pv.Product!.Id,
                Name = pv.Product.Name,
                Description = pv.Product.Description,
                ImageUrl = pv.Product.ImageUrl,
                BasePrice = pv.Product.BasePrice
            }).ToList()
        };
    }
}
