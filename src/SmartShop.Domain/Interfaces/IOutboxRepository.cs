using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
    Task<List<OutboxMessage>> GetPendingBatchAsync(int batchSize, CancellationToken ct = default);
    void Update(OutboxMessage message);
}
