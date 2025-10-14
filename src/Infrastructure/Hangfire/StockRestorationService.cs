using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureBase.Infrastructure.Hangfire;

public class StockRestorationService : IStockRestorationService
{

    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IPlayGroundService _playGroundService;
    public StockRestorationService(
        IBackgroundJobClient backgroundJobClient, IPlayGroundService playGroundService)
    {
        _backgroundJobClient = backgroundJobClient;
        _playGroundService = playGroundService;
    }

    public Task<string> ScheduleStockRestorationAsync(Guid orderId, int delayMinutes = 5)
    {
        var jobId = _backgroundJobClient.Schedule(
            () => _playGroundService.PrintHelloAsync(),
            TimeSpan.FromMinutes(delayMinutes));

        return Task.FromResult(jobId);
    }

    public async Task CancelStockRestorationAsync(Guid orderId)
    {
        // Lấy API giám sát của Hangfire từ storage hiện tại
        var monitorApi = JobStorage.Current.GetMonitoringApi();

        // Lấy tất cả job đang được lên lịch
        var scheduledJobs = monitorApi.ScheduledJobs(0, int.MaxValue);

        // int deletedCount = 0;    
        //
        // foreach (var (jobId, jobDetails) in scheduledJobs)
        // {
        //     var job = jobDetails.Job;
        //     
        //     bool match = job
        //     
        // }

        await Task.CompletedTask;
    }

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
