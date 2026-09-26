using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class OutboxRepository(ApplicationDbContext context) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken ct = default)
        => await context.OutboxMessages.AddAsync(message, ct);

    public async Task<List<OutboxMessage>> GetPendingBatchAsync(int batchSize, CancellationToken ct = default)
        => await context.OutboxMessages
            .Where(m => m.Status == OutboxStatus.Pending)
            .OrderBy(m => m.OccurredAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public void Update(OutboxMessage message) => context.OutboxMessages.Update(message);
}
