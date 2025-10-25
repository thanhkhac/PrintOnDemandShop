using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Enums;

namespace CleanArchitectureBase.Application.Orders.User.Queries;

public class CheckOrderStatusPaidQuery : IRequest<bool>
{
    public Guid OrderId { get; set; }
}

public class CheckOrderStatusPaidQueryHandler : IRequestHandler<CheckOrderStatusPaidQuery, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public CheckOrderStatusPaidQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(CheckOrderStatusPaidQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.CreatedByUser)
            .Where(o => o.Id == request.OrderId && o.CreatedBy == _currentUser.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order == null)
            throw new ErrorCodeException(ErrorCodes.ORDER_NOT_FOUND);

        if (order.PaymentStatus == nameof(OrderPaymentStatus.ONLINE_PAYMENT_PAID))
        {
            return true;
        }

        return false;
    }
}
