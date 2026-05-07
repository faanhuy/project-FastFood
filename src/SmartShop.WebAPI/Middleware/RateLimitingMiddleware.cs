using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.WebAPI.Options;

namespace SmartShop.WebAPI.Middleware;

public class RateLimitingMiddleware(
    RequestDelegate next,
    IRateLimitStore store,
    IOptions<RateLimitOptions> options,
    ILogger<RateLimitingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Static policy map: path (lowercase) → policy name
    // null = exempt
    private static readonly Dictionary<string, string?> PathPolicies = new()
    {
        ["/api/auth/login"]                     = "Auth",
        ["/api/auth/register"]                  = "Auth",
        ["/api/ai/search"]                      = "AI",
        ["/api/ai/chat"]                        = "AI",
        ["/api/ai/generate-description"]        = "AI",
        ["/api/payments/vnpay/callback"]        = null,  // exempt
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var opts = options.Value;

        if (!opts.Enabled)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.ToString().ToLowerInvariant();

        // Determine policy
        string? policyName = ResolvePolicy(path, context.Request.Method);

        // null = exempt path
        if (policyName is null)
        {
            await next(context);
            return;
        }

        // No rule configured for this policy → pass through
        if (!opts.Rules.TryGetValue(policyName, out var rule))
        {
            await next(context);
            return;
        }

        var clientKey = GetClientKey(context);
        var storeKey = $"{policyName}:{clientKey}";
        var window = TimeSpan.FromSeconds(rule.WindowSeconds);

        var (count, resetAt) = await store.IncrementAsync(storeKey, window, context.RequestAborted);

        var remaining = Math.Max(0, rule.PermitLimit - count);
        var resetUnix = resetAt.ToUnixTimeSeconds();

        context.Response.Headers["X-RateLimit-Limit"] = rule.PermitLimit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = resetUnix.ToString();

        if (count > rule.PermitLimit)
        {
            var retryAfter = (long)Math.Ceiling((resetAt - DateTimeOffset.UtcNow).TotalSeconds);
            retryAfter = Math.Max(1, retryAfter);

            context.Response.Headers["Retry-After"] = retryAfter.ToString();
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            logger.LogWarning(
                "Rate limit exceeded for client '{ClientKey}' on policy '{Policy}'. Count={Count}, Limit={Limit}.",
                clientKey, policyName, count, rule.PermitLimit);

            var response = ApiResponse<object>.Fail(
                $"Quá nhiều yêu cầu. Vui lòng thử lại sau {retryAfter} giây.");

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
            return;
        }

        await next(context);
    }

    private static string? ResolvePolicy(string path, string method)
    {
        // Exact match first
        if (PathPolicies.TryGetValue(path, out var exactPolicy))
            return exactPolicy;

        // Special case: POST /api/orders only
        if (path == "/api/orders" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            return "Orders";

        // No match → no rate limit applied
        return string.Empty; // non-null empty means "no policy" → pass through
    }

    private static string GetClientKey(HttpContext context)
    {
        var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = !string.IsNullOrEmpty(forwarded)
            ? forwarded.Split(',')[0].Trim()
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return $"ip:{ip}";
    }
}
