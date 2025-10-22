using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Orders.Admin.Commands;

[Authorize(Roles = Roles.Administrator)]
public class UpdateOrderStatusCommand : IRequest
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsRestock { get; set; } = true;
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

        var oldStatus = Enum.Parse<OrderStatus>(order.Status!) ;
        var newStatus = request.Status;

        // Validate status transitions
        ValidateStatusTransition(oldStatus, newStatus);

        if (oldStatus != newStatus)
        {
            await HandleOrderStateChange(order, newStatus, request.IsRestock, cancellationToken);
        }
        
        if(newStatus == OrderStatus.REJECTED)
        {
            order.PaymentStatus = nameof(OrderPaymentStatus.REJECTED);
        }
        
        order.Status = newStatus.ToString();
        
        if (!string.IsNullOrEmpty(request.Notes))
        {
            order.AdminNote = request.Notes;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleOrderStateChange(
        Order order, 
        OrderStatus newStatus, 
        bool isRestock,
        CancellationToken cancellationToken)
    {
        if ((newStatus == OrderStatus.REJECTED || newStatus == OrderStatus.RETURNED) && isRestock)
        {
            await RestoreProductStock(order, cancellationToken);
        }
    }
    
    
    private async Task RestoreProductStock(Order order, CancellationToken cancellationToken)
    {
        var variantIds = order.Items.Select(x => x.ProductVariantId).ToList();
        
        if (variantIds.Count == 0)
            return;

        var variants = await _context.ProductVariants
            .Where(x => variantIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

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

    private static void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus  newStatus)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            // Khi admin chưa xác nhận
            [OrderStatus.PENDING]  = [OrderStatus.ACCEPTED, OrderStatus.REJECTED],
            
            [OrderStatus.ACCEPTED] = [OrderStatus.PROCESSING, OrderStatus.REJECTED],

            // Khi đang xử lý, có thể chuyển sang gửi hàng hoặc hủy
            [OrderStatus.PROCESSING] = [OrderStatus.SHIPPED, OrderStatus.REJECTED],
            
            // Khi đơn hàng đã gửi đi nhưng bị trả về
            [OrderStatus.SHIPPED] = [OrderStatus.RETURNED],
        };

        if (!validTransitions.TryGetValue(currentStatus, out var allowedTargets))
        {
            throw new ErrorCodeException(
                ErrorCodes.COMMON_INVALID_MODEL,
                new { currentStatus },
                $"Unknown order status: {currentStatus}"
            );
        }

        if (!allowedTargets.Contains(newStatus))
        {
            throw new ErrorCodeException(
                ErrorCodes.COMMON_INVALID_MODEL,
                new { currentStatus, newStatus },
                $"Invalid transition from {currentStatus} → {newStatus}"
            );
        }
    }
}
