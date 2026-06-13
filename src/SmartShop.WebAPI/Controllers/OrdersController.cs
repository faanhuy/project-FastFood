using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Common;
using SmartShop.Application.Features.Orders;
using SmartShop.Application.Features.Orders.Commands.AdminPartialRefundOrder;
using SmartShop.Application.Features.Orders.Commands.AdminRefundOrder;
using SmartShop.Application.Features.Orders.Commands.BulkUpdateOrders;
using SmartShop.Application.Features.Orders.Commands.CancelOrder;
using SmartShop.Application.Features.Orders.Commands.PlaceOrder;
using SmartShop.Application.Features.Orders.Commands.UpdateOrderStatus;
using SmartShop.Application.Features.Orders.Queries.GetAllOrders;
using SmartShop.Application.Features.Orders.Queries.GetMyOrders;
using SmartShop.Application.Features.Orders.Queries.GetOrderById;
using SmartShop.Application.Features.Orders.Queries.GetOrderTimeline;
using SmartShop.Domain.Interfaces;
using SmartShop.Domain.Enums;

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
            new PlaceOrderCommand(CurrentUserId, request.StoreId, request.AddressId,
                request.Notes, request.CouponCode, request.UsePoints, request.PaymentMethod), ct);
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

    /// <summary>Huỷ đơn hàng (chỉ khi Pending, chủ đơn)</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<object?>>> Cancel(Guid id, CancellationToken ct)
    {
        await mediator.Send(new CancelOrderCommand(id, CurrentUserId), ct);
        return Ok(ApiResponse.Ok());
    }

    // ── Admin endpoints ───────────────────────────────────────────────────

    /// <summary>Lấy tất cả đơn hàng (Admin only)</summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? createdAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDirection = "desc",
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAllOrdersQuery(page, pageSize, status, search, createdAfter, createdBefore, sortBy, sortDirection), ct);
        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
    }

    /// <summary>Cập nhật trạng thái đơn hàng (Admin only)</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(
        Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateOrderStatusCommand(id, request.Status), ct);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    /// <summary>Bulk update orders (confirm/ship/deliver/cancel)</summary>
    [HttpPost("admin/bulk-actions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<BulkActionResult>>> BulkUpdateOrders(
        [FromBody] BulkUpdateOrdersRequest request,
        CancellationToken ct)
    {
        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new BulkUpdateOrdersCommand(request.OrderIds, request.Action, adminUserId), ct);
        return Ok(ApiResponse<BulkActionResult>.Ok(result));
    }

    /// <summary>Refund order (full refund) - Admin only</summary>
    [HttpPost("admin/{orderId:guid}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> RefundOrder(
        Guid orderId,
        [FromBody] AdminRefundOrderRequest request,
        CancellationToken ct)
    {
        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new AdminRefundOrderCommand(orderId, adminUserId, request.RefundNote), ct);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    /// <summary>Partial refund order (refund selected items) - Admin only</summary>
    [HttpPost("admin/{orderId:guid}/partial-refund")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> PartialRefundOrder(
        Guid orderId,
        [FromBody] AdminPartialRefundOrderRequest request,
        CancellationToken ct)
    {
        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new AdminPartialRefundOrderCommand(orderId, adminUserId, request.SelectedItemIds, request.RefundNote), ct);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    /// <summary>Get order timeline (status history + events)</summary>
    [HttpGet("{orderId:guid}/timeline")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<OrderTimelineEventDto>>>> GetOrderTimeline(
        Guid orderId,
        CancellationToken ct)
    {
        var userId = CurrentUserId;
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new GetOrderTimelineQuery(orderId, userId, isAdmin), ct);
        return Ok(ApiResponse<List<OrderTimelineEventDto>>.Ok(result));
    }
}

public record PlaceOrderRequest(
    Guid StoreId,
    Guid AddressId,
    string? Notes,
    string? CouponCode,
    decimal UsePoints = 0,
    PaymentMethod PaymentMethod = PaymentMethod.COD);

public record UpdateOrderStatusRequest(OrderStatus Status);

public record BulkUpdateOrdersRequest(
    List<Guid> OrderIds,
    string Action
);

public record AdminRefundOrderRequest(string? RefundNote);

public record AdminPartialRefundOrderRequest(
    List<Guid> SelectedItemIds,
    string? RefundNote);
