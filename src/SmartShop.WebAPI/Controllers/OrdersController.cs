using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Orders;
using SmartShop.Application.Features.Orders.Commands.PlaceOrder;
using SmartShop.Application.Features.Orders.Queries.GetMyOrders;
using SmartShop.Application.Features.Orders.Queries.GetOrderById;
using SmartShop.Application.Products.Queries.GetProducts;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Đặt hàng từ giỏ hàng hiện tại</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> PlaceOrder(
        [FromBody] PlaceOrderRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new PlaceOrderCommand(CurrentUserId, request.ShippingAddress, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<OrderDto>.Ok(result));
    }

    /// <summary>Lấy danh sách đơn hàng của user hiện tại</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetMyOrdersQuery(CurrentUserId, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
    }

    /// <summary>Lấy chi tiết một đơn hàng</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(CurrentUserId, id), ct);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }
}

public record PlaceOrderRequest(string ShippingAddress, string? Notes);
