using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Orders.Admin.Queries;

[Authorize(Roles = "Admin")]
public class GetOrderDetailQuery : IRequest<OrderDetailResponseDto>
{
    public Guid OrderId { get; set; }
}

public class GetOrderDetailQueryValidator : AbstractValidator<GetOrderDetailQuery>
{
    public GetOrderDetailQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");
    }
}

public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, OrderDetailResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetOrderDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDetailResponseDto> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(x => x.Items)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new ErrorCodeException(ErrorCodes.ORDER_NOT_FOUND, request.OrderId, "Order not found");
        }

        return new OrderDetailResponseDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            RecipientName = order.RecipientName,
            RecipientPhone = order.RecipientPhone,
            RecipientAddress = order.RecipientAddress,
            PaymentMethod = order.PaymentMethod,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            CreatedBy = order.CreatedByUser != null ? new CreatedByDto
            {
                UserId = order.CreatedBy,
                Name = order.CreatedByUser.FullName
            } : null,
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
        };
    }
}
