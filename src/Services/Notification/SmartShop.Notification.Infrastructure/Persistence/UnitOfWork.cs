using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartShop.Notification.Application.Common.Interfaces;
using SmartShop.Notification.Domain.Common.Exceptions;
using SmartShop.Notification.Domain.Entities;

namespace SmartShop.Notification.Infrastructure.Persistence;

public class UnitOfWork(NotificationDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Đọc entity trước khi Clear() để còn biết (SourceOrderId, EventType) nào bị chặn.
            var added = context.ChangeTracker.Entries<NotificationRecord>()
                .Select(e => e.Entity)
                .FirstOrDefault(e => e.SourceOrderId.HasValue);

            context.ChangeTracker.Clear();

            throw new DuplicateNotificationException(added?.SourceOrderId, added?.EventType ?? string.Empty);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
