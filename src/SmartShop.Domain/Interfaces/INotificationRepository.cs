using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Notification notification, CancellationToken ct = default);
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken ct = default);
}
