using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
    Task DeleteAsync(Notification notification, CancellationToken ct = default);
    Task DeleteAllByUserIdAsync(string userId, CancellationToken ct = default);
}
