using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Vouchers.Dtos;

namespace CleanArchitectureBase.Application.Vouchers.Queries;

public class GetVouchersByProductQuery : IRequest<List<VoucherDto>>
{
    public Guid ProductId { get; set; }
    public bool? IsActive { get; set; }
}

public class GetVouchersByProductQueryValidator : AbstractValidator<GetVouchersByProductQuery>
{
    public GetVouchersByProductQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");
    }
}

public class GetVouchersByProductQueryHandler : IRequestHandler<GetVouchersByProductQuery, List<VoucherDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVouchersByProductQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VoucherDto>> Handle(GetVouchersByProductQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Vouchers
            .Where(v => v.ProductVouchers.Any(pv => pv.ProductId == request.ProductId));

        if (request.IsActive.HasValue)
        {
            query = query.Where(v => v.IsActive == request.IsActive.Value);
        }

        var vouchers = await query
            .OrderBy(v => v.Code)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Description = v.Description,
                DiscountType = v.DiscountType,
                DiscountValue = v.DiscountValue,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                UsedCount = v.UsedCount,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt,
                LastModifiedAt = v.LastModifiedAt,
                Products = v.ProductVouchers.Where(pv => pv.Product != null && !pv.Product.IsDeleted).Select(pv => new VoucherProductDto
                {
                    ProductId = pv.ProductId,
                    ProductName = pv.Product!.Name,
                    ProductImageUrl = pv.Product.ImageUrl
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return vouchers;
    }
}
