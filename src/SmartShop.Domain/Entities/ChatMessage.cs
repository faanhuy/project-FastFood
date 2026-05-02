using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class ChatMessage : BaseAuditableEntity
{
    public Guid ChatSessionId { get; private set; }
    public string Role { get; private set; } = string.Empty;      // "user" | "assistant"
    public string Content { get; private set; } = string.Empty;

    private ChatMessage() { }

    public static ChatMessage Create(Guid chatSessionId, string role, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new ChatMessage
        {
            ChatSessionId = chatSessionId,
            Role = role,
            Content = content
        };
    }
}
