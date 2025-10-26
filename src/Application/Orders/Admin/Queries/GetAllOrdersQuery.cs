using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Orders.Admin.Queries;

[Authorize(Roles = Roles.Administrator)]
public class GetAllOrdersQuery : PaginatedQuery, IRequest<PaginatedList<OrderDetailResponseDto>>
{
    /// <summary>
    /// PENDING, // Nếu đơn hàng đang chờ phía admin xác nhận 
    /// REJECTED, // Admin từ chối đơn hàng
    /// PROCESSING, // Đang xử lý, đóng gói
    /// SHIPPED, // Đã chuyển đi
    /// DELIVERED, // Bên vận chuyển đã báo đã tới nơi
    /// CONFIRM_RECEIVED, // User xác nhận đã nhận hàng
    /// CANCELLED, // Đã hủy trước khi admin xác nhận
    /// </summary>
    public OrderStatus? Status { get; set; }
    public OrderPaymentStatus? PaymentStatus { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
}

public class GetAllOrdersQueryValidator : PaginatedQueryValidator<GetAllOrdersQuery>
{
    public GetAllOrdersQueryValidator()
    {    
        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid order status");

        RuleFor(x => x.PaymentStatus)
            .IsInEnum()
            .When(x => x.PaymentStatus.HasValue)
            .WithMessage("Invalid payment status");

        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.CustomerName));

        RuleFor(x => x.CustomerEmail)
            .MaximumLength(255)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail));
    }
}

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PaginatedList<OrderDetailResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<OrderDetailResponseDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(x => x.Items)
            .Include(x => x.CreatedByUser)
            .Where(o => o.Status != nameof(OrderStatus.CANCELLED) )
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.ToString());
        }

        if (request.PaymentStatus.HasValue)
        {
            query = query.Where(x => x.PaymentStatus == request.PaymentStatus.ToString());
        }

        if (!string.IsNullOrEmpty(request.CustomerName))
        {
            query = query.Where(x => x.CreatedByUser!.FullName!.Contains(request.CustomerName));
        }

        if (!string.IsNullOrEmpty(request.CustomerEmail))
        {
            query = query.Where(x => x.CreatedByUser!.Email!.Contains(request.CustomerEmail));
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        return await query
            .Select(order => new OrderDetailResponseDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                RecipientName = order.RecipientName,
                RecipientPhone = order.RecipientPhone,
                RecipientAddress = order.RecipientAddress,
                PaymentMethod = order.PaymentMethod,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                UserFeedback = order.UserFeedback,
                Rating = order.Rating, // Giữ nguyên nullable, không cần check > 0
                IsFeedback = order.IsFeedback,
                CreatedBy = new CreatedByDto
                {
                    UserId = order.CreatedBy,
                    Name = order.CreatedByUser != null ? order.CreatedByUser.FullName : string.Empty,
                    Email = order.CreatedByUser != null ? order.CreatedByUser.Email : string.Empty,
                },
                PaymentCode = order.PaymentCode,
                Items = order.Items.Select(item => new OrderItemResponseDto
                {
                    Id = item.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductDesignId = item.ProductDesignId,
                    VoucherId = item.VoucherId,
                    Name = item.Name,
                    VariantSku = item.VariantSku,
                    ImageUrl = item.VariantImageUrl,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    SubTotal = item.SubTotal,
                    DiscountAmount = item.DiscountAmount,
                    TotalAmount = item.TotalAmount,
                    VoucherCode = item.VoucherCode,
                    VoucherDiscountAmount = item.VoucherDiscountAmount,
                    VoucherDiscountPercent = item.VoucherDiscountPercent
                }).ToList()
            })
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
