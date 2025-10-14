namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IStockRestorationService
{
    /// <summary>
    /// Schedule a job to restore stock if order is not paid within timeout
    /// </summary>
    /// <param name="orderId">Order ID to check</param>
    /// <param name="delayMinutes">Minutes to wait before restoring stock</param>
    /// <returns>Job ID for tracking</returns>
    Task<string> ScheduleStockRestorationAsync(Guid orderId, int delayMinutes = 5);
    
    /// <summary>
    /// Cancel scheduled stock restoration job (when payment is completed)
    /// </summary>
    /// <param name="orderId">Order ID to cancel restoration for</param>
    Task CancelStockRestorationAsync(Guid orderId);
    
    /// <summary>
    /// Execute stock restoration for unpaid order
    /// </summary>
    /// <param name="orderId">Order ID to restore stock for</param>
    Task RestoreStockForUnpaidOrderAsync(Guid orderId);
}
