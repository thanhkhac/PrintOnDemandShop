using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Vouchers.Dtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Vouchers.Queries;

// [Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class SearchVouchersQuery : IRequest<PaginatedList<VoucherDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? DiscountType { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ProductId { get; set; }
}

public class SearchVouchersQueryValidator : AbstractValidator<SearchVouchersQuery>
{
    public SearchVouchersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}

public class SearchVouchersQueryHandler : IRequestHandler<SearchVouchersQuery, PaginatedList<VoucherDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchVouchersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<VoucherDto>> Handle(SearchVouchersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Vouchers.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(v => 
                (v.Code != null && v.Code.Contains(request.SearchTerm)) ||
                (v.Description != null && v.Description.Contains(request.SearchTerm)));
        }

        if (!string.IsNullOrEmpty(request.DiscountType))
        {
            query = query.Where(v => v.DiscountType == request.DiscountType);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(v => v.IsActive == request.IsActive.Value);
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(v => v.ProductVouchers.Any(pv => pv.ProductId == request.ProductId.Value));
        }

        // Order by creation date descending
        query = query.OrderByDescending(v => v.CreatedAt);

        var projectedQuery = query.Select(v => new VoucherDto
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
        });

        return await projectedQuery.PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
