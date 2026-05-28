using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.AI.Commands.SendMessage;

public class SendMessageCommandHandler(
    IFaqDocumentRepository faqRepo,
    IChatSessionRepository chatSessionRepo,
    IChatbotService chatbotService,
    IUnitOfWork uow)
    : IRequestHandler<SendMessageCommand, SendMessageResponseDto>
{
    public async Task<SendMessageResponseDto> Handle(SendMessageCommand cmd, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmd.Message);

        // 1. Load hoặc tạo ChatSession
        bool isNew;
        ChatSession session;

        if (cmd.SessionId.HasValue)
        {
            var existing = await chatSessionRepo.GetByIdAsync(cmd.SessionId.Value, ct);
            if (existing is not null)
            {
                session = existing;
                isNew = false;
            }
            else
            {
                session = ChatSession.Create();
                isNew = true;
            }
        }
        else
        {
            session = ChatSession.Create();
            isNew = true;
        }

        // 2. Load all active FAQ documents
        var faqs = await faqRepo.GetAllActiveAsync(ct);

        // 3. Lấy 5 messages gần nhất từ session history
        var history = session.Messages
            .OrderBy(m => m.CreatedAt)
            .TakeLast(5)
            .Select(m => (m.Role, m.Content))
            .ToList();

        // 4. Gọi chatbot service
        var reply = await chatbotService.GenerateReplyAsync(cmd.Message, faqs, history, ct);

        // 5. Lưu user message + assistant reply vào session
        session.AddMessage("user", cmd.Message);
        session.AddMessage("assistant", reply);

        if (isNew)
            await chatSessionRepo.AddAsync(session, ct);
        else
            await chatSessionRepo.UpdateAsync(session, ct);

        await uow.SaveChangesAsync(ct);

        // 6. Return DTO
        return new SendMessageResponseDto(reply, session.Id, ["faq"]);
    }
}
