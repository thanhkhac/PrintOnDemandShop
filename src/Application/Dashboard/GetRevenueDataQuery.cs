using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Enums;

namespace CleanArchitectureBase.Application.Dashboard;

public class RevenueDataDto
{
    public long TotalRevenue { get; set; }
    public int TotalOrder { get; set; }
    public int TotalQuantity { get; set; }
}

public class GetRevenueDataQuery : IRequest<RevenueDataDto>
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? OrderType { get; set; }
}

public class GetRevenueDataQueryValidator : AbstractValidator<GetRevenueDataQuery>
{
    public GetRevenueDataQueryValidator()
    {
        // Month (nullable, nếu có thì phải từ 1-12)
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .When(x => x.Month.HasValue)
            .WithMessage("Month must be between 1 and 12.");

        // Year (nullable, nếu có thì phải >= 2000 và <= năm hiện tại)
        RuleFor(x => x.Year)
            .InclusiveBetween(0, int.MaxValue)
            .When(x => x.Year.HasValue)
            .WithMessage($"Year must be between 2000 and {int.MaxValue}.");

        // OrderType (nullable, nếu có thì chỉ nhận 1 trong 3 giá trị)
        RuleFor(x => x.OrderType)
            .Must(type => string.IsNullOrEmpty(type)
                          || type.Equals("ALL", StringComparison.OrdinalIgnoreCase)
                          || type.Equals("ONLINE", StringComparison.OrdinalIgnoreCase)
                          || type.Equals("COD", StringComparison.OrdinalIgnoreCase))
            .WithMessage("OrderType must be one of: ALL, ONLINE, COD.");
    }
}

public class GetRevenueDataQueryHandler : IRequestHandler<GetRevenueDataQuery, RevenueDataDto>
{
    private readonly IApplicationDbContext _context;

    public GetRevenueDataQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }


    public  Task<RevenueDataDto> Handle(GetRevenueDataQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(x => x.Items)
            .Where(x => x.Status == nameof(OrderStatus.CONFIRM_RECEIVED) );
        
        if(request.Month.HasValue && request.Year.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Month == request.Month.Value && x.CreatedAt.Year == request.Year.Value);
        }
        
        if (!string.IsNullOrEmpty(request.OrderType) &&
            !request.OrderType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => nameof(x.PaymentMethod) == request.OrderType.ToUpper());
        }
        
        var orders = query
            .Select(x => new
            {
                x.TotalAmount,
                Quantity = x.Items.Sum(i => i.Quantity)
            }).ToList();
            
        var dto = new RevenueDataDto
        {
            TotalOrder = orders.Count(),
            TotalQuantity = orders.Sum(x => x.Quantity),
            TotalRevenue = orders.Sum(x => x.TotalAmount)
        };

        return Task.FromResult(dto);    
    }
}
