using MediatR;

namespace SmartShop.Application.Features.AI.Queries.GetChatHistory;

public record GetChatHistoryQuery(Guid SessionId) : IRequest<IReadOnlyList<ChatMessageDto>>;

public record ChatMessageDto(string Role, string Content, DateTime CreatedAt);
