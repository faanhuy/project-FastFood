using System.Diagnostics;

namespace SmartShop.WebAPI.Middleware;

public class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    private static readonly HashSet<string> ExcludedPaths = new()
    {
        "/health",
        "/health/detail",
        "/metrics",
        "/swagger",
        "/hubs"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var shouldLog = !ExcludedPaths.Any(path.StartsWith);

        // Generate or retrieve correlation ID
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var existingId)
            ? existingId.FirstOrDefault() ?? Guid.NewGuid().ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-Id", correlationId);

        if (shouldLog)
        {
            var method = context.Request.Method;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var elapsed = stopwatch.ElapsedMilliseconds;

                logger.LogInformation(
                    "[{Method}] {Path} -> {StatusCode} in {ElapsedMs}ms [{CorrelationId}]",
                    method,
                    path,
                    statusCode,
                    elapsed,
                    correlationId);
            }
        }
        else
        {
            await next(context);
        }
    }
}
