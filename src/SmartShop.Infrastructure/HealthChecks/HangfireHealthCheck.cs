using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SmartShop.Infrastructure.HealthChecks;

public class HangfireHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var api = JobStorage.Current.GetMonitoringApi();
            var stats = api.GetStatistics();

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    $"Hangfire is operational. Succeeded: {stats.Succeeded}, Failed: {stats.Failed}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy($"Hangfire health check failed: {ex.Message}", ex));
        }
    }
}
