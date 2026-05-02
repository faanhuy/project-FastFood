using SmartShop.Domain.Entities;

namespace SmartShop.Application.Common.Interfaces;

public interface IChatbotService
{
    Task<string> GenerateReplyAsync(
        string userMessage,
        IReadOnlyList<FaqDocument> context,
        IReadOnlyList<(string Role, string Content)> history,
        CancellationToken ct = default);
}
