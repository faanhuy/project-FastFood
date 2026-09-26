using System.Collections.Concurrent;
using SmartShop.Application.Common.Interfaces;

namespace SmartShop.Infrastructure.Locking;

/// <summary>
/// Khóa trong process — dùng khi Redis được tắt bằng config (dev, mặc định <c>Cache:Enabled=false</c>).
/// Đúng cho 1 instance; KHÔNG bảo vệ được khi chạy nhiều instance (cần <see cref="RedisDistributedLock"/>).
/// Có ngữ nghĩa giống bản Redis (non-blocking, TTL, chỉ chủ khóa mới nhả được) để dev không lệch prod.
/// </summary>
public class InMemoryDistributedLock(TimeProvider? timeProvider = null) : IDistributedLock
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    private readonly ConcurrentDictionary<string, Entry> _locks = new();

    // record → so sánh theo giá trị; Token (Guid) khác nhau nên mỗi lần acquire là một Entry duy nhất.
    private sealed record Entry(Guid Token, DateTimeOffset ExpiresAt);

    public Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var entry = new Entry(Guid.NewGuid(), _time.GetUtcNow() + expiry);

        while (true)
        {
            if (_locks.TryAdd(key, entry))
                return Task.FromResult<IAsyncDisposable?>(new LockHandle(_locks, key, entry));

            // Key vừa bị nhả giữa TryAdd và TryGetValue → thử lại.
            if (!_locks.TryGetValue(key, out var existing))
                continue;

            if (existing.ExpiresAt > _time.GetUtcNow())
                return Task.FromResult<IAsyncDisposable?>(null);

            // Khóa cũ đã hết hạn: thay thế nguyên tử (thua race → vòng lặp thử lại và thấy khóa mới của người thắng).
            if (_locks.TryUpdate(key, entry, existing))
                return Task.FromResult<IAsyncDisposable?>(new LockHandle(_locks, key, entry));
        }
    }

    private sealed class LockHandle(ConcurrentDictionary<string, Entry> locks, string key, Entry entry) : IAsyncDisposable
    {
        private int _released;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                // Xóa có điều kiện theo (key, entry): nếu khóa đã hết hạn và bị người khác giành, không xóa nhầm.
                locks.TryRemove(new KeyValuePair<string, Entry>(key, entry));
            }

            return ValueTask.CompletedTask;
        }
    }
}
