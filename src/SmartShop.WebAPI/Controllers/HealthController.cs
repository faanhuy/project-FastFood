using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartShop.Application.Common.Models;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api")]
public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var result = await healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new
        {
            status = result.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = result.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            }).ToList()
        };

        // Return 503 (Service Unavailable) if unhealthy
        if (result.Status == HealthStatus.Unhealthy)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);

        return Ok(response);
    }

    [HttpGet("health/detail")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetHealthDetail(CancellationToken cancellationToken)
    {
        var result = await healthCheckService.CheckHealthAsync(cancellationToken);
        var startTime = Process.GetCurrentProcess().StartTime;
        var uptime = DateTime.UtcNow - startTime;

        var response = new
        {
            status = result.Status.ToString(),
            timestamp = DateTime.UtcNow,
            uptime = FormatUptime(uptime),
            checks = result.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = $"{entry.Value.Duration.TotalMilliseconds:F0}ms",
                description = entry.Value.Description,
                error = entry.Value.Exception?.Message
            }).ToList(),
            system = new
            {
                memoryMb = GC.GetTotalMemory(false) / 1024 / 1024,
                processorCount = Environment.ProcessorCount,
                processorCountPhysical = Environment.ProcessorCount
            }
        };

        return Ok(response);
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";

        if (uptime.TotalMinutes >= 1)
            return $"{(int)uptime.TotalMinutes}m {uptime.Seconds}s";

        return $"{uptime.Seconds}s";
    }
}
