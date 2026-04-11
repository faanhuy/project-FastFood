using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Reviews;
using SmartShop.Application.Features.Reviews.Commands.AddReview;
using SmartShop.Application.Features.Reviews.Commands.DeleteReview;
using SmartShop.Application.Features.Reviews.Queries;
using SmartShop.Application.Products.Queries.GetProducts;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin =>
        User.IsInRole("Admin");

    /// <summary>Lấy danh sách đánh giá đã duyệt của một sản phẩm</summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductReviewsQuery(productId, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<ReviewDto>>.Ok(result));
    }

    /// <summary>Gửi đánh giá sản phẩm (1 user chỉ đánh giá 1 lần)</summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> AddReview(
        [FromBody] AddReviewRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddReviewCommand(CurrentUserId, request.ProductId, request.Rating, request.Comment), ct);
        return StatusCode(201, ApiResponse<ReviewDto>.Ok(result));
    }

    /// <summary>Xóa đánh giá (owner hoặc Admin)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteReviewCommand(id, CurrentUserId, IsAdmin), ct);
        return Ok(ApiResponse.Ok("Đánh giá đã được xóa."));
    }
}

public record AddReviewRequest(Guid ProductId, int Rating, string Comment);
