using FluentAssertions;
using Moq;
using Xunit;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.AI.Commands.SendMessage;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.AI;

public class SendMessageCommandHandlerTests
{
    private readonly Mock<IFaqDocumentRepository> _faqRepo = new();
    private readonly Mock<IChatSessionRepository> _chatSessionRepo = new();
    private readonly Mock<IChatbotService> _chatbotService = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    public SendMessageCommandHandlerTests()
    {
        // Default: no FAQs, save succeeds
        _faqRepo
            .Setup(r => r.GetAllActiveAsync(default))
            .ReturnsAsync(Array.Empty<FaqDocument>());

        _uow
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);
    }

    private SendMessageCommandHandler CreateHandler() =>
        new(_faqRepo.Object, _chatSessionRepo.Object, _chatbotService.Object, _uow.Object);

    // ---------------------------------------------------------------
    // 1. New session — SessionId is null
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_NewSession_NullSessionId_CreatesSessionAndReturnsReply()
    {
        const string aiReply = "AI reply";

        _chatbotService
            .Setup(s => s.GenerateReplyAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<FaqDocument>>(),
                It.IsAny<IReadOnlyList<(string Role, string Content)>>(),
                default))
            .ReturnsAsync(aiReply);

        var cmd = new SendMessageCommand { Message = "Hello", SessionId = null };

        var result = await CreateHandler().Handle(cmd, default);

        // chatSessionRepo.AddAsync called; UpdateAsync never called
        _chatSessionRepo.Verify(r => r.AddAsync(It.IsAny<ChatSession>(), default), Times.Once);
        _chatSessionRepo.Verify(r => r.UpdateAsync(It.IsAny<ChatSession>(), default), Times.Never);

        // SaveChanges called exactly once
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);

        // Reply content
        result.Reply.Should().Be(aiReply);

        // Sources contain "faq"
        result.Sources.Should().ContainSingle().Which.Should().Be("faq");

        // SessionId is a non-empty GUID
        result.SessionId.Should().NotBe(Guid.Empty);
    }

    // ---------------------------------------------------------------
    // 2. Existing SessionId — session found in repo
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_ExistingSessionId_SessionFound_UpdatesSession()
    {
        var existingSession = ChatSession.Create();

        _chatSessionRepo
            .Setup(r => r.GetBySessionIdAsync(existingSession.SessionId, default))
            .ReturnsAsync(existingSession);

        _chatbotService
            .Setup(s => s.GenerateReplyAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<FaqDocument>>(),
                It.IsAny<IReadOnlyList<(string Role, string Content)>>(),
                default))
            .ReturnsAsync("reply");

        var cmd = new SendMessageCommand { Message = "Hi", SessionId = existingSession.SessionId };

        var result = await CreateHandler().Handle(cmd, default);

        // UpdateAsync called; AddAsync never called
        _chatSessionRepo.Verify(r => r.UpdateAsync(existingSession, default), Times.Once);
        _chatSessionRepo.Verify(r => r.AddAsync(It.IsAny<ChatSession>(), default), Times.Never);

        // SessionId in response matches the existing session
        result.SessionId.Should().Be(existingSession.SessionId);
    }

    // ---------------------------------------------------------------
    // 3. Existing SessionId — session NOT found → create new
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_ExistingSessionId_SessionNotFound_CreatesNewSession()
    {
        var missingId = Guid.NewGuid();

        _chatSessionRepo
            .Setup(r => r.GetBySessionIdAsync(missingId, default))
            .ReturnsAsync((ChatSession?)null);

        _chatbotService
            .Setup(s => s.GenerateReplyAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<FaqDocument>>(),
                It.IsAny<IReadOnlyList<(string Role, string Content)>>(),
                default))
            .ReturnsAsync("reply");

        var cmd = new SendMessageCommand { Message = "Hi", SessionId = missingId };

        var result = await CreateHandler().Handle(cmd, default);

        // New session added
        _chatSessionRepo.Verify(r => r.AddAsync(It.IsAny<ChatSession>(), default), Times.Once);
        _chatSessionRepo.Verify(r => r.UpdateAsync(It.IsAny<ChatSession>(), default), Times.Never);

        // A fresh SessionId is assigned (not the missing one)
        result.SessionId.Should().NotBe(Guid.Empty);
        result.SessionId.Should().NotBe(missingId);
    }

    // ---------------------------------------------------------------
    // 4. Only the last 5 messages are forwarded to chatbot
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithExistingMessages_PassesLast5ToService()
    {
        var existingSession = ChatSession.Create();

        // Add 7 messages with staggered timestamps so ordering is deterministic
        for (var i = 0; i < 7; i++)
        {
            existingSession.AddMessage("user", $"message {i}");
        }

        _chatSessionRepo
            .Setup(r => r.GetBySessionIdAsync(existingSession.SessionId, default))
            .ReturnsAsync(existingSession);

        IReadOnlyList<(string Role, string Content)>? capturedHistory = null;

        _chatbotService
            .Setup(s => s.GenerateReplyAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<FaqDocument>>(),
                It.IsAny<IReadOnlyList<(string Role, string Content)>>(),
                default))
            .Callback<string, IReadOnlyList<FaqDocument>, IReadOnlyList<(string Role, string Content)>, CancellationToken>(
                (_, _, history, _) => capturedHistory = history)
            .ReturnsAsync("reply");

        var cmd = new SendMessageCommand { Message = "question", SessionId = existingSession.SessionId };

        await CreateHandler().Handle(cmd, default);

        capturedHistory.Should().NotBeNull();
        capturedHistory!.Count.Should().Be(5);
    }

    // ---------------------------------------------------------------
    // 5. Empty / whitespace message throws ArgumentException
    // ---------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyOrWhitespaceMessage_ThrowsArgumentException(string emptyMessage)
    {
        var cmd = new SendMessageCommand { Message = emptyMessage };

        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ---------------------------------------------------------------
    // 6. All active FAQs are forwarded to chatbot
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_FaqsLoadedAndPassedToService()
    {
        var faqs = new List<FaqDocument>
        {
            FaqDocument.Create("shipping", "When does it arrive?", "3-5 business days."),
            FaqDocument.Create("returns",  "Can I return?",        "Yes, within 30 days."),
            FaqDocument.Create("payment",  "What payments?",       "Card, PayPal, COD.")
        };

        _faqRepo
            .Setup(r => r.GetAllActiveAsync(default))
            .ReturnsAsync(faqs);

        IReadOnlyList<FaqDocument>? capturedFaqs = null;

        _chatbotService
            .Setup(s => s.GenerateReplyAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<FaqDocument>>(),
                It.IsAny<IReadOnlyList<(string Role, string Content)>>(),
                default))
            .Callback<string, IReadOnlyList<FaqDocument>, IReadOnlyList<(string Role, string Content)>, CancellationToken>(
                (_, faqList, _, _) => capturedFaqs = faqList)
            .ReturnsAsync("reply");

        var cmd = new SendMessageCommand { Message = "question" };

        await CreateHandler().Handle(cmd, default);

        capturedFaqs.Should().NotBeNull();
        capturedFaqs!.Should().HaveCount(3);
        capturedFaqs.Should().BeEquivalentTo(faqs);
    }
}
