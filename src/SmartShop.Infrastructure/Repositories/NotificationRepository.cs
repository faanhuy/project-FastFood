using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class NotificationRepository(ApplicationDbContext context) : INotificationRepository
{
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await context.Notifications.AddAsync(notification, ct);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public Task DeleteAsync(Notification notification, CancellationToken ct = default)
    {
        context.Notifications.Remove(notification);
        return Task.CompletedTask;
    }

    public async Task DeleteAllByUserIdAsync(string userId, CancellationToken ct = default)
    {
        var notifications = await context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync(ct);
        context.Notifications.RemoveRange(notifications);
    }
}
