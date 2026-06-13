using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Cart;
using SmartShop.Application.Features.Cart.Commands.AddComboToCart;
using SmartShop.Application.Features.Cart.Commands.AddToCart;
using SmartShop.Application.Features.Cart.Commands.ClearCart;
using SmartShop.Application.Features.Cart.Commands.AddFromOrder;
using SmartShop.Application.Features.Cart.Commands.RemoveCartItemById;
using SmartShop.Application.Features.Cart.Commands.RemoveFromCart;
using SmartShop.Application.Features.Cart.Commands.UpdateCartItemById;
using SmartShop.Application.Features.Cart.Commands.UpdateCartItem;
using SmartShop.Application.Features.Cart.Queries.GetCart;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCartQuery(CurrentUserId), ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart(
        [FromBody] AddToCartRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddToCartCommand(CurrentUserId, request.ProductId, request.Quantity, request.SizeId), ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    [HttpPost("combo-items")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddComboToCart(
        [FromBody] AddComboToCartRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddComboToCartCommand(CurrentUserId, request.ComboId, request.Quantity), ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    /// <summary>Cập nhật số lượng theo CartItem.Id (dùng cho cả product và combo)</summary>
    [HttpPut("items/line/{cartItemId:guid}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItemById(
        Guid cartItemId, [FromBody] UpdateByLineIdRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateCartItemByIdCommand(CurrentUserId, cartItemId, request.Quantity), ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    /// <summary>Xoá theo CartItem.Id (dùng cho cả product và combo)</summary>
    [HttpDelete("items/line/{cartItemId:guid}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> RemoveCartItemById(
        Guid cartItemId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveCartItemByIdCommand(CurrentUserId, cartItemId), ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    /// <summary>Cập nhật số lượng theo productId (backward compat)</summary>
    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem(
        Guid productId, [FromBody] UpdateQuantityRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateCartItemCommand(CurrentUserId, productId, request.Quantity, request.SizeId), ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    /// <summary>Xoá theo productId (backward compat)</summary>
    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> RemoveFromCart(
        Guid productId, [FromQuery] Guid? sizeId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveFromCartCommand(CurrentUserId, productId, sizeId), ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    [HttpPost("from-order/{orderId:guid}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddFromOrder(Guid orderId, CancellationToken ct)
    {
        var result = await mediator.Send(new AddFromOrderCommand(CurrentUserId, orderId), ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object?>>> ClearCart(CancellationToken ct)
    {
        await mediator.Send(new ClearCartCommand(CurrentUserId), ct);
        return Ok(ApiResponse.Ok("Giỏ hàng đã được xoá."));
    }
}

public record AddToCartRequest(Guid ProductId, int Quantity, Guid? SizeId = null);
public record AddComboToCartRequest(Guid ComboId, int Quantity = 1);
public record UpdateQuantityRequest(int Quantity, Guid? SizeId = null);
public record UpdateByLineIdRequest(int Quantity);
