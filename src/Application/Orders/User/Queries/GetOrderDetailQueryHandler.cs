using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Orders.User.Queries;

public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, OrderDetailResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetOrderDetailQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OrderDetailResponseDto> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.CreatedByUser)
            .Where(o => o.Id == request.OrderId && o.CreatedBy == _currentUser.UserId)
            .Where(o => o.Status != nameof(OrderStatus.CANCELLED) )
            .FirstOrDefaultAsync(cancellationToken);

        if (order == null)
            throw new ErrorCodeException(ErrorCodes.ORDER_NOT_FOUND);

        return new OrderDetailResponseDto
        {
            OrderId = order!.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            RecipientName = order.RecipientName,
            RecipientPhone = order.RecipientPhone,
            RecipientAddress = order.RecipientAddress,
            PaymentMethod = order.PaymentMethod,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            IsFeedback = order.IsFeedback,
            Items = order.Items.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductVariantId = oi.ProductVariantId,
                    ProductDesignId = oi.ProductDesignId,
                    VoucherId = oi.VoucherId,
                    Name = oi.Name,
                    VariantSku = oi.VariantSku,
                    ImageUrl = oi.VariantImageUrl,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    SubTotal = oi.SubTotal,
                    DiscountAmount = oi.DiscountAmount,
                    TotalAmount = oi.TotalAmount,
                    VoucherCode = oi.VoucherCode,
                    VoucherDiscountAmount = oi.VoucherDiscountAmount,
                    VoucherDiscountPercent = oi.VoucherDiscountPercent
                })
                .ToList(),
            CreatedBy = new CreatedByDto
            {
                UserId = order.CreatedBy,
                Name = order.CreatedByUser?.FullName ?? string.Empty,
                Email = order.CreatedByUser?.FullName
            },
            PaymentStatus = order.PaymentStatus,
            PaymentCode = order.PaymentCode
        };
    }
}
