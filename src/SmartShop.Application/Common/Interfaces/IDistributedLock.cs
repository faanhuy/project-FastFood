namespace SmartShop.Application.Common.Interfaces;

/// <summary>
/// Khóa non-blocking theo key (vd <c>lock:inventory:{storeId}:{productId}</c>) để chặn 2 request
/// cùng giữ chỗ 1 sản phẩm tại một thời điểm. Không chờ: khóa đang có người giữ → trả về <c>null</c> ngay,
/// caller quyết định (PlaceOrder ném <c>ConflictException</c>).
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// Thử giành khóa <paramref name="key"/>, tự hết hạn sau <paramref name="expiry"/> phòng khi process chết giữa chừng.
    /// </summary>
    /// <returns>
    /// Handle giành được khóa — <c>await using</c>/<c>DisposeAsync</c> để nhả (idempotent, không throw);
    /// <c>null</c> nếu khóa đang do request khác giữ.
    /// </returns>
    /// <exception cref="SmartShop.Domain.Common.Exceptions.ServiceUnavailableException">
    /// Backend lock lỗi. Fail-safe: KHÔNG được coi như "giành được khóa" — caller phải từ chối order.
    /// </exception>
    Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default);
}
