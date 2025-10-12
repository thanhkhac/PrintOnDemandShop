using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Orders.User.Queries;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PaginatedList<OrderDetailResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetMyOrdersQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<OrderDetailResponseDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Where(o => o.CreatedBy == _currentUser.UserId)
            .AsQueryable();

        // Filter by status if provided
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(o => o.Status == request.Status);
        }

        // Order by newest first

        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Include(o => o.Items)
            .Select(o => new OrderDetailResponseDto
            {
                OrderId = o.Id,
                OrderDate = o.CreatedAt,
                Status = o.Status,
                RecipientName = o.RecipientName,
                RecipientPhone = o.RecipientPhone,
                RecipientAddress = o.RecipientAddress,
                PaymentMethod = o.PaymentMethod,
                SubTotal = o.SubTotal,
                DiscountAmount = o.DiscountAmount,
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(oi => new OrderItemResponseDto
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
                    VoucherCode = oi.Voucher != null ? oi.Voucher.Code : null,
                    VoucherDiscountAmount = oi.VoucherDiscountAmount,
                    VoucherDiscountPercent = oi.VoucherDiscountPercent
                }).ToList(),
                CreatedBy = new CreatedByDto
                {
                    UserId = o.CreatedBy!,
                    Name = o.CreatedByUser != null ? o.CreatedByUser.FullName : string.Empty
                }
            })
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return orders;
    }
}
