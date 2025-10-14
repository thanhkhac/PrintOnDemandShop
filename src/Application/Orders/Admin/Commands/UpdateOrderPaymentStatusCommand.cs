using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Orders.Admin.Commands;

[Authorize(Roles = Roles.Administrator)]
public class UpdateOrderPaymentStatusCommand : IRequest
{
    public Guid OrderId { get; set; }
    public OrderPaymentStatus PaymentStatus { get; set; }
}

public class UpdateOrderPaymentStatusCommandValidator : AbstractValidator<UpdateOrderPaymentStatusCommand>
{
    public UpdateOrderPaymentStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");

        RuleFor(x => x.PaymentStatus)
            .IsInEnum()
            .WithMessage("Invalid payment status");
    }
}

public class UpdateOrderPaymentStatusCommandHandler : IRequestHandler<UpdateOrderPaymentStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IHangfireService _hangfireService;

    public UpdateOrderPaymentStatusCommandHandler(
        IApplicationDbContext context,
        IHangfireService hangfireService)
    {
        _context = context;
        _hangfireService = hangfireService;
    }

    public async Task Handle(UpdateOrderPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new ErrorCodeException(ErrorCodes.ORDER_NOT_FOUND, request.OrderId, "Order not found");
        }

        var oldPaymentStatus = order.PaymentStatus;
        var newPaymentStatus = request.PaymentStatus.ToString();

        // Validate payment status transitions
        ValidatePaymentStatusTransition(oldPaymentStatus, newPaymentStatus);

        order.PaymentStatus = newPaymentStatus;
        
        //TODO: Liệu có nên để admin có thể đặt Online payment thành paid không
        // Hủy job hoàn stock nếu thanh toán thành công
        if (oldPaymentStatus == nameof(OrderPaymentStatus.ONLINE_PAYMENT_AWAITING) && 
            newPaymentStatus == nameof(OrderPaymentStatus.ONLINE_PAYMENT_PAID))
        {
            await _hangfireService.CancelStockRestorationAsync(order.Id);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void ValidatePaymentStatusTransition(string? currentPaymentStatus, string newPaymentStatus)
    {
        if (string.IsNullOrEmpty(currentPaymentStatus))
            return;

        var current = Enum.Parse<OrderPaymentStatus>(currentPaymentStatus);
        var target = Enum.Parse<OrderPaymentStatus>(newPaymentStatus);

        // Define invalid payment status transitions
        var invalidTransitions = new Dictionary<OrderPaymentStatus, OrderPaymentStatus[]>
        {
            [OrderPaymentStatus.ONLINE_PAYMENT_PAID] = [OrderPaymentStatus.ONLINE_PAYMENT_AWAITING],
            [OrderPaymentStatus.REFUNDED] = [OrderPaymentStatus.ONLINE_PAYMENT_AWAITING, OrderPaymentStatus.ONLINE_PAYMENT_PAID, OrderPaymentStatus.COD],
            [OrderPaymentStatus.COD] = [OrderPaymentStatus.ONLINE_PAYMENT_AWAITING, OrderPaymentStatus.ONLINE_PAYMENT_PAID]
        };

        if (invalidTransitions.ContainsKey(current) && invalidTransitions[current].Contains(target))
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_MODEL, 
                new { currentPaymentStatus, newPaymentStatus }, 
                $"Cannot change payment status from {current} to {target}");
        }

        // Business rules
        if (current == OrderPaymentStatus.REFUNDED)
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_MODEL, 
                new { currentPaymentStatus, newPaymentStatus }, 
                "Cannot change payment status once it has been refunded");
        }
    }
}
