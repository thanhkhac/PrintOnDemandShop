using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Orders.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Orders.Admin.Queries;

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
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(x => x.Status == request.Status);
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
                RecipientName = order.RecipientName,
                RecipientPhone = order.RecipientPhone,
                RecipientAddress = order.RecipientAddress,
                PaymentMethod = order.PaymentMethod,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                CreatedBy = new CreatedByDto
                {
                    UserId = order.CreatedBy,
                    Name = order.CreatedByUser != null ? order.CreatedByUser.FullName : string.Empty
                },
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
