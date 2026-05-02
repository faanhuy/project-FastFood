using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class ChatSession : BaseAuditableEntity
{
    private readonly List<ChatMessage> _messages = [];

    public Guid SessionId { get; private set; }   // public-facing GUID
    public Guid? UserId { get; private set; }      // nullable — support anonymous

    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    private ChatSession() { }

    public static ChatSession Create(Guid? userId = null)
    {
        return new ChatSession
        {
            SessionId = Guid.NewGuid(),
            UserId = userId
        };
    }

    public void AddMessage(string role, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        _messages.Add(ChatMessage.Create(Id, role, content));
    }
}
