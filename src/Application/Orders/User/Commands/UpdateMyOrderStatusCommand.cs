using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.IClients;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Orders.User.Commands;

[Authorize]
public class UpdateMyOrderStatusCommand : IRequest
{
    public Guid OrderId { get; set; }
    public UserOrderAction Action { get; set; }
    // public string? Feedback { get; set; }
    // public int? Rating { get; set; }
}

public enum UserOrderAction
{
    CANCEL,           // Hủy đơn hàng
    CONFIRM_RECEIVED  // Xác nhận đã nhận hàng
}

public class UpdateMyOrderStatusCommandValidator : AbstractValidator<UpdateMyOrderStatusCommand>
{
    public UpdateMyOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");

        RuleFor(x => x.Action)
            .IsInEnum()
            .WithMessage("Invalid action");

        // RuleFor(x => x.Rating)
        //     .InclusiveBetween(1, 5)
        //     .When(x => x.Action == UserOrderAction.CONFIRM_RECEIVED && x.Rating.HasValue)
        //     .WithMessage("Rating must be between 1 and 5");
        //
        // RuleFor(x => x.Feedback)
        //     .MaximumLength(500)
        //     .When(x => !string.IsNullOrEmpty(x.Feedback))
        //     .WithMessage("Feedback cannot exceed 500 characters");
    }
}

public class UpdateMyOrderStatusCommandHandler : IRequestHandler<UpdateMyOrderStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    
    private List<Guid> _newProductVariantIds = new();
    private readonly IAiClient _aiClient;


    public UpdateMyOrderStatusCommandHandler(IApplicationDbContext context, IUser currentUser, IAiClient aiClient)
    {
        _context = context;
        _currentUser = currentUser;
        _aiClient = aiClient;
    }

    public async Task Handle(UpdateMyOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CreatedBy == _currentUser.UserId, 
                cancellationToken);

        if (order == null)
        {
            throw new ErrorCodeException(ErrorCodes.ORDER_NOT_FOUND, request.OrderId, 
                "Order not found or you don't have permission to update this order");
        }

        var oldStatus = order.Status;
        
        switch (request.Action)
        {
            case UserOrderAction.CANCEL:
                await HandleCancelOrder(order, oldStatus, cancellationToken);
                break;
                
            case UserOrderAction.CONFIRM_RECEIVED:
                HandleConfirmReceived(order, oldStatus, "", 1);
                break;
                
            default:
                throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_MODEL, request.Action, 
                    "Invalid action");
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        
        
        try
        {
            if (_newProductVariantIds.Any())
            {
                await _aiClient.CreateProduct(new
                {
                    product_variant_ids = _newProductVariantIds
                });
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task HandleCancelOrder(Order order, string? currentStatus, CancellationToken cancellationToken)
    {
        // Chỉ cho phép hủy khi đơn hàng ở trạng thái PENDING
        var cancellableStatuses = new[] 
        { 
            nameof(OrderStatus.PENDING)
        };

        if (!cancellableStatuses.Contains(currentStatus))
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_MODEL, 
                new { currentStatus, action = "CANCEL" }, 
                "Cannot cancel order in current status. Only PENDING orders can be cancelled.");
        }

        // Hoàn lại stock khi user hủy đơn
        await RestoreStock(order, cancellationToken);
        
        // Xử lý payment status nếu đã thanh toán
        HandlePaymentRefunding(order);
        
        order.Status = nameof(OrderStatus.CANCELLED);
    }

    private static void HandlePaymentRefunding(Order order)
    {
        // Nếu đơn hàng đã được thanh toán online, chuyển về REFUNDING
        if (order.PaymentStatus == nameof(OrderPaymentStatus.ONLINE_PAYMENT_PAID))
        {
            order.PaymentStatus = nameof(OrderPaymentStatus.REFUNDING);
        }
        // COD và AWAITING_ONLINE_PAYMENT không cần xử lý refund vì chưa thanh toán
    }

    private static void HandleConfirmReceived(Order order, string? currentStatus, string? feedback, int? rating)
    {
        // Chỉ cho phép xác nhận nhận hàng khi đơn hàng đã được giao
        if (currentStatus != nameof(OrderStatus.SHIPPED))
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_MODEL, 
                new { currentStatus, action = "CONFIRM_RECEIVED" }, 
                "Can only confirm receipt when order is delivered");
        }

        // Thay đổi status thành CONFIRM_RECEIVED
        order.Status = nameof(OrderStatus.CONFIRM_RECEIVED);
        
        // Lưu feedback nếu có
        if (!string.IsNullOrEmpty(feedback))
        {
            order.UserFeedback = feedback;
        }
        
        // Lưu rating nếu có và hợp lệ (1-5)
        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
        {
            order.Rating = rating.Value;
        }
        // Nếu không có rating hoặc rating không hợp lệ, giữ nguyên Rating = null
    }

    private async Task RestoreStock(Order order, CancellationToken cancellationToken)
    {
        var variantIds = order.Items.Select(x => x.ProductVariantId).ToList();
        var variants = await _context.ProductVariants
            .Where(x => variantIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var item in order.Items)
        {
            var variant = variants.FirstOrDefault(x => x.Id == item.ProductVariantId);
            if (variant != null)
            {
                if (variant.Stock == 0 && item.Quantity > 0)
                {
                    _newProductVariantIds.Add(item.ProductVariantId);
                }
                
                variant.Stock += item.Quantity;
                _context.ProductVariants.Update(variant);
            }
        }
    }
}
