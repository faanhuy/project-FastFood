using MediatR;

namespace SmartShop.Application.Features.AI.Commands.SendMessage;

public record SendMessageCommand : IRequest<SendMessageResponseDto>
{
    public string Message { get; init; } = string.Empty;
    public Guid? SessionId { get; init; }
}

public record SendMessageResponseDto(
    string Reply,
    Guid SessionId,
    string[] Sources);
