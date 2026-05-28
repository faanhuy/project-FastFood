using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.AI.Queries.GetChatHistory;

public class GetChatHistoryQueryHandler(IChatSessionRepository chatSessionRepo)
    : IRequestHandler<GetChatHistoryQuery, IReadOnlyList<ChatMessageDto>>
{
    public async Task<IReadOnlyList<ChatMessageDto>> Handle(GetChatHistoryQuery query, CancellationToken ct)
    {
        var session = await chatSessionRepo.GetByIdAsync(query.SessionId, ct)
            ?? throw new NotFoundException("ChatSession", query.SessionId);

        return session.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto(m.Role, m.Content, m.CreatedAt))
            .ToList()
            .AsReadOnly();
    }
}
