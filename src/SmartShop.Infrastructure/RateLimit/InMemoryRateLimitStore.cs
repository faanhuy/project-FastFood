using System.Collections.Concurrent;
using SmartShop.Application.Common.Interfaces;

namespace SmartShop.Infrastructure.RateLimit;

public class InMemoryRateLimitStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<string, Entry> _store = new();

    private sealed class Entry
    {
        public long Count;
        public DateTimeOffset ResetAt;
    }

    public Task<(long Count, DateTimeOffset ResetAt)> IncrementAsync(
        string key, TimeSpan window, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var entry = _store.AddOrUpdate(
            key,
            // add factory
            _ => new Entry { Count = 1, ResetAt = now.Add(window) },
            // update factory
            (_, existing) =>
            {
                lock (existing)
                {
                    // Lazy cleanup: if window has expired, reset
                    if (existing.ResetAt <= now)
                    {
                        existing.Count = 1;
                        existing.ResetAt = now.Add(window);
                    }
                    else
                    {
                        existing.Count++;
                    }
                    return existing;
                }
            });

        long count;
        DateTimeOffset resetAt;
        lock (entry)
        {
            count = entry.Count;
            resetAt = entry.ResetAt;
        }

        return Task.FromResult((count, resetAt));
    }
}
