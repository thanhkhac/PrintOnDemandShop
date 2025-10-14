using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Orders.Interfaces;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureBase.Infrastructure.Hangfire;

public class HangfireService : IHangfireService
{

    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IPlayGroundService _playGroundService;
    private readonly IOrderService _orderService;
    public HangfireService(
        IBackgroundJobClient backgroundJobClient, IPlayGroundService playGroundService, IOrderService orderService)
    {
        _backgroundJobClient = backgroundJobClient;
        _playGroundService = playGroundService;
        _orderService = orderService;
    }

    public Task<string> ScheduleStockRestorationAsync(Guid orderId, int delayMinutes = 5)
    {
        var jobId = _backgroundJobClient.Schedule(
            () => _orderService.RestockOrder(orderId),
            TimeSpan.FromSeconds(10));

        return Task.FromResult(jobId);
    }

    public async Task CancelStockRestorationAsync(Guid orderId)
    {
        var content = orderId.ToString();
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty.", nameof(content));

        var monitoringApi = JobStorage.Current.GetMonitoringApi();

        // Lấy tất cả job đang được lên lịch (scheduled)
        var scheduledJobs = monitoringApi.ScheduledJobs(0, int.MaxValue);

        int deletedCount = 0;

        foreach (var (jobId, jobDetails) in scheduledJobs)
        {
            var job = jobDetails.Job;
            if (job == null || job.Args == null) continue;

            bool match = job.Args.Any(arg =>
                arg != null &&
                arg.ToString()!.Contains(content, StringComparison.OrdinalIgnoreCase)
            );

            if (match)
            {
                BackgroundJob.Delete(jobId);
                deletedCount++;
            }
        }

        Console.WriteLine($"Deleted {deletedCount} job(s) containing '{content}'.");
        await Task.CompletedTask;
    }



}
