using SmartShop.Inventory.Domain.Common.Exceptions;

namespace SmartShop.Inventory.Application.Common;

/// <summary>
/// Chạy lại toàn bộ action khi gặp <see cref="ConcurrencyException"/> (request khác vừa sửa cùng dòng tồn kho).
/// Action PHẢI tự tải lại dữ liệu ở mỗi lần chạy — UnitOfWork đã ChangeTracker.Clear() sau khi xung đột.
/// </summary>
public static class ConcurrencyRetry
{
    public static async Task<T> RunAsync<T>(Func<Task<T>> action, int maxAttempts = 2)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (ConcurrencyException) when (attempt < maxAttempts)
            {
                // thử lại với dữ liệu mới
            }
        }
    }
}
