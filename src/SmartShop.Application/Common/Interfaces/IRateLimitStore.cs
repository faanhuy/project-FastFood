namespace SmartShop.Application.Common.Interfaces;

public interface IRateLimitStore
{
    Task<(long Count, DateTimeOffset ResetAt)> IncrementAsync(
        string key, TimeSpan window, CancellationToken ct = default);
}
