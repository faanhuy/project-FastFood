using System.Net;

namespace SmartShop.WebAPI.Middleware;

public class MetricsAuthMiddleware(
    RequestDelegate next,
    ILogger<MetricsAuthMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Only check metrics endpoint
        if (context.Request.Path.StartsWithSegments("/metrics"))
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            // Allow loopback (127.0.0.1, ::1)
            if (remoteIp == null || (!IPAddress.IsLoopback(remoteIp) && !IsPrivateNetwork(remoteIp)))
            {
                logger.LogWarning(
                    "Unauthorized access to /metrics from IP: {RemoteIp}",
                    remoteIp?.ToString() ?? "Unknown");

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Access denied");
                return;
            }
        }

        await next(context);
    }

    private static bool IsPrivateNetwork(IPAddress ip)
    {
        // IPv4 private ranges: 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168);
        }

        // IPv6 link-local and private (fc00::/7)
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var bytes = ip.GetAddressBytes();
            return (bytes[0] & 0xfe) == 0xfc; // fc00::/7
        }

        return false;
    }
}
