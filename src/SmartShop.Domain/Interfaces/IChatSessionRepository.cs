using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IChatSessionRepository
{
    Task<ChatSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ChatSession session, CancellationToken ct = default);
    Task UpdateAsync(ChatSession session, CancellationToken ct = default);
}
