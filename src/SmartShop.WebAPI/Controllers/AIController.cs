using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.DTOs;
using SmartShop.Application.Features.AI;
using SmartShop.Application.Features.AI.Commands.GenerateDescription;
using SmartShop.Application.Features.AI.Commands.SendMessage;
using SmartShop.Application.Features.AI.Queries.GetChatHistory;
using SmartShop.Application.Features.AI.Queries.GetRecommendations;
using SmartShop.Application.Features.AI.Queries.SemanticSearch;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/ai")]
public class AIController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Semantic search sử dụng AI embeddings (public, không cần auth).
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SemanticSearchResultDto>>>> SemanticSearch(
        [FromBody] SemanticSearchRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SemanticSearchQuery(request.Query, request.TopN, request.StoreId), ct);
        return Ok(ApiResponse<IReadOnlyList<SemanticSearchResultDto>>.Ok(result));
    }

    /// <summary>
    /// Lấy gợi ý sản phẩm tương tự dựa trên AI embedding (public).
    /// </summary>
    [HttpGet("recommendations/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductDto>>>> GetRecommendations(
        Guid productId, [FromQuery] int count = 5, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRecommendationsQuery(productId, count), ct);
        return Ok(ApiResponse<IReadOnlyList<ProductDto>>.Ok(result));
    }

    /// <summary>
    /// Tạo mô tả sản phẩm bằng AI (yêu cầu đăng nhập).
    /// </summary>
    [HttpPost("generate-description")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> GenerateDescription(
        [FromBody] GenerateDescriptionCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Gửi tin nhắn đến AI chatbot và nhận phản hồi dựa trên FAQ (public).
    /// </summary>
    [HttpPost("chat")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<SendMessageResponseDto>.Ok(result));
    }

    /// <summary>
    /// Lấy lịch sử hội thoại theo sessionId (public).
    /// </summary>
    [HttpGet("chat/{sessionId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetChatHistory(Guid sessionId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetChatHistoryQuery(sessionId), ct);
        return Ok(ApiResponse<IReadOnlyList<ChatMessageDto>>.Ok(result));
    }
}

public record SemanticSearchRequest(string Query, int TopN = 10, Guid? StoreId = null);
