using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Orders.Interfaces;
using CleanArchitectureBase.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureBase.Infrastructure;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<OrderService> _logger;
    public OrderService(IApplicationDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Queue("stock")]
    public async Task RestockOrder(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Starting stock restoration check for order {OrderId}", orderId);

            var order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for stock restoration", orderId);
                return;
            }

            // Chỉ hoàn stock nếu order vẫn đang chờ thanh toán và chưa được xử lý
            var shouldRestoreStock = order.PaymentStatus == nameof(OrderPaymentStatus.ONLINE_PAYMENT_AWAITING) &&
                                     order.Status == nameof(OrderStatus.PENDING);

            if (!shouldRestoreStock)
            {
                _logger.LogInformation("Order {OrderId} payment status: {PaymentStatus}, order status: {OrderStatus}. No stock restoration needed.",
                    orderId, order.PaymentStatus, order.Status);
                return;
            }

            // Hoàn lại stock
            var variantIds = order.Items.Select(x => x.ProductVariantId).ToList();
            var variants = await _context.ProductVariants
                .Where(x => variantIds.Contains(x.Id))
                .ToListAsync();

            foreach (var item in order.Items)
            {
                var variant = variants.FirstOrDefault(x => x.Id == item.ProductVariantId);
                if (variant != null)
                {
                    variant.Stock += item.Quantity;
                    _context.ProductVariants.Update(variant);

                    _logger.LogInformation("Restored {Quantity} stock for variant {VariantId} from unpaid order {OrderId}",
                        item.Quantity, variant.Id, orderId);
                }
            }

            // Cập nhật trạng thái order thành EXPIRED
            order.Status = nameof(OrderStatus.EXPIRED);
            order.PaymentStatus = nameof(OrderPaymentStatus.EXPIRED);

            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("Successfully restored stock and marked order {OrderId} as EXPIRED due to payment timeout", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore stock for order {OrderId}", orderId);
            throw;
        }
    }
}
