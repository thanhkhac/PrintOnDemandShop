using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Orders.Interfaces;
using CleanArchitectureBase.Domain.Enums;
using CleanArchitectureBase.Infrastructure.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureBase.Infrastructure;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderService> _logger;
    public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

[Queue("stock")]
[DisableConcurrentExecution(timeoutInSeconds: 120)]
[AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
public async Task RestockOrder(Guid orderId)
{
    _logger.LogInformation("Starting stock restoration for order {OrderId}", orderId);

    await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

    try
    {
        var order = await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return;
        }

        var shouldRestoreStock = order.PaymentStatus == nameof(OrderPaymentStatus.ONLINE_PAYMENT_AWAITING) &&
                                order.Status == nameof(OrderStatus.PENDING);

        if (!shouldRestoreStock)
        {
            _logger.LogInformation("Order {OrderId} does not require stock restoration", orderId);
            return;
        }

        var variantIds = order.Items.Select(x => x.ProductVariantId).ToList();
        if (!variantIds.Any())
        {
            _logger.LogWarning("No variants found for order {OrderId}", orderId);
            return;
        }

        // ✅ Lock và load variants cùng lúc bằng LINQ to SQL (EF sẽ generate đúng MySQL syntax)
        var variants = await _context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync();

        // Update stock for each item
        foreach (var item in order.Items)
        {
            var variant = variants.FirstOrDefault(x => x.Id == item.ProductVariantId);
            if (variant == null)
            {
                _logger.LogError("ProductVariant {VariantId} not found for order {OrderId}", 
                    item.ProductVariantId, orderId);
                continue;
            }

            variant.Stock += item.Quantity;

            _logger.LogInformation("Restored {Quantity} stock for variant {VariantId} from order {OrderId}",
                item.Quantity, item.ProductVariantId, orderId);
        }

        // Update order status
        order.Status = nameof(OrderStatus.EXPIRED);
        order.PaymentStatus = nameof(OrderPaymentStatus.EXPIRED);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        _logger.LogInformation("Stock restoration completed for order {OrderId}", orderId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to restock order {OrderId}", orderId);
        await transaction.RollbackAsync();
        throw;
    }
}
}
