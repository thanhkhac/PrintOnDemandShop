using CleanArchitectureBase.Application.Common.Interfaces;
using Hangfire;

namespace CleanArchitectureBase.Infrastructure.Hangfire;

public class HangFireService : IHangFireService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    
    public HangFireService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }
    

        public Task DeleteJobByArgument(string content)
        {
            var connection = JobStorage.Current.GetConnection();
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            var scheduledJobs = monitoringApi.ScheduledJobs(0, int.MaxValue);
            
            foreach (var kv in scheduledJobs)
            {
                var jobId = kv.Key;
                var jobData = kv.Value.Job;

                if (jobData?.Args != null && jobData.Args.Any(a => a != null && a.ToString()!.Contains(content)))
                {
                    BackgroundJob.Delete(jobId);
                }    
            }
            
            return Task.CompletedTask;
        }
}
