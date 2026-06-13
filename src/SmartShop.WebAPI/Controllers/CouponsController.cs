using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Common;
using SmartShop.Application.Features.Coupons;
using SmartShop.Application.Features.Coupons.Commands.BulkDeleteCoupons;
using SmartShop.Application.Features.Coupons.Commands.CreateCoupon;
using SmartShop.Application.Features.Coupons.Commands.DeleteCoupon;
using SmartShop.Application.Features.Coupons.Queries;
using SmartShop.Application.Features.Coupons.Queries.GetCoupons;
using SmartShop.Domain.Interfaces;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/coupons")]
[Authorize]
public class CouponsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lấy danh sách coupon (Admin, có phân trang và lọc)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResult<CouponResponse>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isExpired = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCouponsQuery(page, pageSize, search, isExpired), ct);
        return Ok(ApiResponse<PagedResult<CouponResponse>>.Ok(result));
    }

    /// <summary>Tạo coupon mới (Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CouponResponse>>> Create(
        [FromBody] CreateCouponCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), ApiResponse<CouponResponse>.Ok(result));
    }

    /// <summary>Xoá coupon theo code (Admin)</summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(string code, CancellationToken ct)
    {
        await mediator.Send(new DeleteCouponCommand(code), ct);
        return Ok(ApiResponse.Ok());
    }

    /// <summary>Kiểm tra và tính toán giá trị coupon</summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<ValidateCouponResponse>>> Validate(
        [FromBody] ValidateCouponRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new ValidateCouponQuery(request.Code, request.OrderTotal, CurrentUserId), ct);
        return Ok(ApiResponse<ValidateCouponResponse>.Ok(result));
    }

    /// <summary>Bulk delete coupons (Admin)</summary>
    [HttpPost("bulk-actions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<BulkActionResult>>> BulkDeleteCoupons(
        [FromBody] BulkDeleteCouponsRequest request,
        CancellationToken ct)
    {
        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(new BulkDeleteCouponsCommand(request.CouponIds, adminUserId), ct);
        return Ok(ApiResponse<BulkActionResult>.Ok(result));
    }
}

public record ValidateCouponRequest(string Code, decimal OrderTotal);

public record BulkDeleteCouponsRequest(List<Guid> CouponIds);
