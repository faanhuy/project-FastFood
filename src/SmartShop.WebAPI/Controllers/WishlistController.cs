using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Wishlist.Commands.AddToWishlist;
using SmartShop.Application.Features.Wishlist.Commands.RemoveFromWishlist;
using SmartShop.Application.Features.Wishlist.Queries.GetWishlist;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistController(IMediator mediator) : ControllerBase
{
    /// <summary>Lấy danh sách yêu thích của user hiện tại</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WishlistItemDto>>>> GetWishlist(CancellationToken ct)
    {
        var result = await mediator.Send(new GetWishlistQuery(), ct);
        return Ok(result);
    }

    /// <summary>Thêm sản phẩm vào danh sách yêu thích</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> AddToWishlist(
        [FromBody] AddToWishlistRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddToWishlistCommand(request.ProductId), ct);
        return Ok(result);
    }

    /// <summary>Xóa sản phẩm khỏi danh sách yêu thích</summary>
    [HttpDelete("{productId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromWishlist(Guid productId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveFromWishlistCommand(productId), ct);
        return Ok(result);
    }
}

public record AddToWishlistRequest(Guid ProductId);
