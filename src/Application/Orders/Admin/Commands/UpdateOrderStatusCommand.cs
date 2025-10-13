using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Orders.Admin.Commands;

[Authorize(Roles = "Admin")]
public class UpdateOrderStatusCommand : IRequest
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid order status");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters");
    }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new ErrorCodeException(ErrorCodes.ORDER_NOT_FOUND, request.OrderId, "Order not found");
        }

        var oldStatus = order.Status;
        var newStatus = request.Status.ToString();

        // Validate status transitions
        ValidateStatusTransition(oldStatus, newStatus);

        // Xử lý logic hoàn lại stock khi hủy/từ chối order
        await HandleStockRestoration(order, oldStatus, newStatus, cancellationToken);

        order.Status = newStatus;
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleStockRestoration(Order order, string? oldStatus, string newStatus, CancellationToken cancellationToken)
    {
        // Chỉ hoàn lại stock khi chuyển từ trạng thái active sang cancelled/rejected
        var activeStatuses = new[] { 
            nameof(OrderStatus.PENDING), 
            nameof(OrderStatus.PROCESSING), 
            nameof(OrderStatus.AWAITING_PAYMENT),
            nameof(OrderStatus.SHIPPED) 
        };
        
        var cancelledStatuses = new[] { 
            nameof(OrderStatus.CANCELLED), 
            nameof(OrderStatus.REFUNDED) 
        };

        bool shouldRestoreStock = activeStatuses.Contains(oldStatus) && cancelledStatuses.Contains(newStatus);

        if (shouldRestoreStock)
        {
            // Lấy tất cả ProductVariant liên quan đến order items
            var variantIds = order.Items.Select(x => x.ProductVariantId).ToList();
            var variants = await _context.ProductVariants
                .Where(x => variantIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            // Hoàn lại stock cho từng variant
            foreach (var item in order.Items)
            {
                var variant = variants.FirstOrDefault(x => x.Id == item.ProductVariantId);
                if (variant != null)
                {
                    variant.Stock += item.Quantity;
                    _context.ProductVariants.Update(variant);
                }
            }
        }
    }

    private static void ValidateStatusTransition(string? currentStatus, string newStatus)
    {
        if (string.IsNullOrEmpty(currentStatus))
            return;

        var current = Enum.Parse<OrderStatus>(currentStatus);
        var target = Enum.Parse<OrderStatus>(newStatus);

        // Define invalid transitions
        var invalidTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.CANCELLED] = [OrderStatus.PENDING, OrderStatus.PROCESSING, OrderStatus.SHIPPED, OrderStatus.DELIVERED],
            [OrderStatus.DELIVERED] = [OrderStatus.PENDING, OrderStatus.PROCESSING, OrderStatus.SHIPPED],
            [OrderStatus.REFUNDED] = [OrderStatus.PENDING, OrderStatus.PROCESSING, OrderStatus.SHIPPED, OrderStatus.DELIVERED]
        };

        if (invalidTransitions.ContainsKey(current) && invalidTransitions[current].Contains(target))
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_MODEL, new { currentStatus, newStatus }, 
                $"Cannot change order status from {current} to {target}");
        }

        // Additional business rules
        if (current == OrderStatus.SHIPPED && target == OrderStatus.CANCELLED)
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_MODEL, new { currentStatus, newStatus }, 
                "Cannot cancel a shipped order");
        }
    }
}
