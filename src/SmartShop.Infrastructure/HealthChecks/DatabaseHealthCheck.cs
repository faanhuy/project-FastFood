using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.HealthChecks;

public class DatabaseHealthCheck(ApplicationDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
                return HealthCheckResult.Unhealthy("Database connection failed");

            return HealthCheckResult.Healthy("Database is operational");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}", ex);
        }
    }
}
