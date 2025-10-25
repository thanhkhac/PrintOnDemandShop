using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Enums;

namespace CleanArchitectureBase.Application.Dashboard;

public class DashBoardData
{
    public int TotalUser { get; set; }
    public int TotalProduct { get; set; }
    public int TotalPendingOrder { get; set; }
    public int TotalAcceptedOrder { get; set; }
    public int TotalProcessingOrder { get; set; }
    public int TotalShippedOrder { get; set; }
    public int TotalConfirmReceived { get; set; }
}

public class GetDashboardDataQuery : IRequest<DashBoardData>
{

}

public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, DashBoardData>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardDataQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashBoardData> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        var totalUser = await _context.DomainUsers.CountAsync(x => x.IsDeleted == false, cancellationToken: cancellationToken);
        var totalProduct = await _context.Products.CountAsync(x => x.IsDeleted == false, cancellationToken: cancellationToken);
        var totalAcceptedOrder = await _context.Orders.CountAsync(x => x.Status == nameof(OrderStatus.ACCEPTED), cancellationToken: cancellationToken);
        var totalPendingOrder = await _context.Orders.CountAsync(x => x.Status == nameof(OrderStatus.PENDING), cancellationToken: cancellationToken);
        var totalProcessingOrder = await _context.Orders.CountAsync(x => x.Status == nameof(OrderStatus.PROCESSING), cancellationToken: cancellationToken);
        var totalShippedOrder = await _context.Orders.CountAsync(x => x.Status == nameof(OrderStatus.SHIPPED), cancellationToken: cancellationToken);
        var totalConfirmReceived = await _context.Orders.CountAsync(x => x.Status == nameof(OrderStatus.CONFIRM_RECEIVED), cancellationToken: cancellationToken);

        var dashboardData = new DashBoardData
        {
            TotalUser = totalUser,
            TotalProduct = totalProduct,
            TotalPendingOrder = totalAcceptedOrder,
            TotalAcceptedOrder = totalPendingOrder,
            TotalProcessingOrder = totalProcessingOrder,
            TotalShippedOrder = totalShippedOrder,
            TotalConfirmReceived = totalConfirmReceived
        };

        return dashboardData;
    }
}
