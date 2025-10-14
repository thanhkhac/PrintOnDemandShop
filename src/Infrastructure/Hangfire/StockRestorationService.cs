using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureBase.Infrastructure.Hangfire;

public class StockRestorationService : IStockRestorationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<StockRestorationService> _logger;
    private readonly IHangFireService _hangFireService;

    public StockRestorationService(
        IApplicationDbContext context,
        ILogger<StockRestorationService> logger,
        IHangFireService hangFireService)
    {
        _context = context;
        _logger = logger;
        _hangFireService = hangFireService;
    }

    public Task<string> ScheduleStockRestorationAsync(Guid orderId, int delayMinutes = 5)
    {
        // Sửa lỗi: TimeSpan.FromMinutes thay vì FromSeconds
        var jobId = BackgroundJob.Schedule(
            () => RestoreStockForUnpaidOrderAsync(orderId),
            TimeSpan.FromSeconds(10)); // Đang để 10s để test

        _logger.LogInformation("Scheduled stock restoration job {JobId} for order {OrderId} in {DelayMinutes} minutes",
            jobId, orderId, delayMinutes);

        return Task.FromResult(jobId);
    }

    public async Task CancelStockRestorationAsync(Guid orderId)
    {
        // Cancel job by searching for jobs with this orderId
        await _hangFireService.DeleteJobByArgument(orderId.ToString());

        _logger.LogInformation("Cancelled stock restoration job for order {OrderId}", orderId);
    }

    // Đảm bảo method này là public và có [AutomaticRetry] attribute cho Hangfire
    [AutomaticRetry(Attempts = 3)]
    public async Task RestoreStockForUnpaidOrderAsync(Guid orderId)
    {
        // try
        // {
        //     _logger.LogInformation("Starting stock restoration check for order {OrderId}", orderId);
        //
        //     var order = await _context.Orders
        //         .Include(x => x.Items)
        //         .FirstOrDefaultAsync(x => x.Id == orderId);
        //
        //     if (order == null)
        //     {
        //         _logger.LogWarning("Order {OrderId} not found for stock restoration", orderId);
        //         return;
        //     }
        //
        //     // Chỉ hoàn stock nếu order vẫn đang chờ thanh toán và chưa được xử lý
        //     var shouldRestoreStock = order.PaymentStatus == nameof(OrderPaymentStatus.ONLINE_PAYMENT_AWAITING) &&
        //                            order.Status == nameof(OrderStatus.PENDING);
        //
        //     if (!shouldRestoreStock)
        //     {
        //         _logger.LogInformation("Order {OrderId} payment status: {PaymentStatus}, order status: {OrderStatus}. No stock restoration needed.",
        //             orderId, order.PaymentStatus, order.Status);
        //         return;
        //     }
        //
        //     // Hoàn lại stock
        //     var variantIds = order.Items.Select(x => x.ProductVariantId).ToList();
        //     var variants = await _context.ProductVariants
        //         .Where(x => variantIds.Contains(x.Id))
        //         .ToListAsync();
        //
        //     foreach (var item in order.Items)
        //     {
        //         var variant = variants.FirstOrDefault(x => x.Id == item.ProductVariantId);
        //         if (variant != null)
        //         {
        //             variant.Stock += item.Quantity;
        //             _context.ProductVariants.Update(variant);
        //
        //             _logger.LogInformation("Restored {Quantity} stock for variant {VariantId} from unpaid order {OrderId}",
        //                 item.Quantity, variant.Id, orderId);
        //         }
        //     }
        //
        //     // Cập nhật trạng thái order thành EXPIRED
        //     order.Status = nameof(OrderStatus.EXPIRED);
        //     order.PaymentStatus = nameof(OrderPaymentStatus.EXPIRED);
        //
        //     await _context.SaveChangesAsync(CancellationToken.None);
        //
        //     _logger.LogInformation("Successfully restored stock and marked order {OrderId} as EXPIRED due to payment timeout", orderId);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Failed to restore stock for order {OrderId}", orderId);
        //     throw;
        // }
        
        Console.WriteLine("=======================");
        Console.WriteLine("HELLO");
        Console.WriteLine("=======================");
        
        await Task.CompletedTask;
    }
}
