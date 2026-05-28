using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class ChatSessionRepository(ApplicationDbContext db) : IChatSessionRepository
{
    public async Task<ChatSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task AddAsync(ChatSession session, CancellationToken ct = default)
    {
        await db.ChatSessions.AddAsync(session, ct);
    }

    public Task UpdateAsync(ChatSession session, CancellationToken ct = default)
    {
        // Session already tracked by EF — calling Update() would mark existing
        // ChatMessages as Modified and cause concurrency conflicts. Instead, only
        // add new messages that aren't yet in the change tracker.
        foreach (var msg in session.Messages)
        {
            if (db.Entry(msg).State == EntityState.Detached)
                db.ChatMessages.Add(msg);
        }
        return Task.CompletedTask;
    }
}
