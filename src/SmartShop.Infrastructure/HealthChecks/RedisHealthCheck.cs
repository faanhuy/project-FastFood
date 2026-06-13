using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Infrastructure.Caching;

namespace SmartShop.Infrastructure.HealthChecks;

public class RedisHealthCheck(ICacheService cacheService) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if Redis is disabled
            if (cacheService is NoOpCacheService)
                return HealthCheckResult.Unhealthy("Redis is disabled (using NoOpCacheService)");

            // Try a simple set/get/remove operation
            const string testKey = "health-check:ping";
            const string testValue = "1";

            await cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(5), cancellationToken);
            var retrieved = await cacheService.GetAsync<string>(testKey, cancellationToken);
            await cacheService.RemoveAsync(testKey, cancellationToken);

            if (retrieved != testValue)
                return HealthCheckResult.Unhealthy("Redis value mismatch during health check");

            return HealthCheckResult.Healthy("Redis is operational");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Redis health check failed: {ex.Message}", ex);
        }
    }
}
