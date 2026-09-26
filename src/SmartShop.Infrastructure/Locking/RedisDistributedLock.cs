using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using StackExchange.Redis;

namespace SmartShop.Infrastructure.Locking;

/// <summary>
/// Khóa phân tán trên Redis: <c>SET key token NX PX expiry</c> (<c>LockTakeAsync</c>).
/// Redis lỗi/timeout → <see cref="ServiceUnavailableException"/> (fail-safe), không bao giờ bỏ qua khóa để đặt hàng tiếp.
/// </summary>
public class RedisDistributedLock(IConnectionMultiplexer redis, ILogger<RedisDistributedLock> logger)
    : IDistributedLock
{
    public async Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Token riêng cho mỗi lần acquire — chỉ chủ khóa (đúng token) mới nhả được, xem LockHandle.
        var token = Guid.NewGuid().ToString("N");
        var db = redis.GetDatabase();

        try
        {
            var acquired = await db.LockTakeAsync(key, token, expiry).WaitAsync(ct);
            return acquired ? new LockHandle(db, key, token, logger) : null;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            logger.LogError(ex, "Redis lock backend failed while acquiring '{Key}'. Rejecting (fail-safe).", key);
            throw new ServiceUnavailableException("Distributed lock backend unavailable.");
        }
    }

    private sealed class LockHandle(IDatabase db, string key, string token, ILogger logger) : IAsyncDisposable
    {
        private int _released;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _released, 1) == 1) return;

            try
            {
                // Compare-and-delete: chỉ xóa khi key vẫn mang token của mình. Nếu TTL đã hết và request khác
                // đã giành lại, DEL vô điều kiện sẽ xóa nhầm khóa của họ. Không nhận CancellationToken:
                // request bị hủy vẫn phải nhả khóa.
                await db.LockReleaseAsync(key, token);
            }
            catch (Exception ex) when (ex is RedisException or TimeoutException)
            {
                // Chạy trong finally của handler — throw ở đây sẽ che exception gốc. Khóa tự hết hạn theo TTL.
                logger.LogWarning(ex, "Failed to release lock '{Key}'. It will expire by TTL.", key);
            }
        }
    }
}
