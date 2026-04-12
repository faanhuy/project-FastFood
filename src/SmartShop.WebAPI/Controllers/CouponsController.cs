using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Coupons;
using SmartShop.Application.Features.Coupons.Commands.CreateCoupon;
using SmartShop.Application.Features.Coupons.Commands.DeleteCoupon;
using SmartShop.Application.Features.Coupons.Queries;
using SmartShop.Application.Features.Coupons.Queries.GetCoupons;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/coupons")]
[Authorize]
public class CouponsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lấy danh sách tất cả coupon (Admin)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CouponResponse>>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCouponsQuery(), ct);
        return Ok(ApiResponse<IEnumerable<CouponResponse>>.Ok(result));
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
        return Ok(ApiResponse<string>.Ok("Đã xoá coupon thành công."));
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
}

public record ValidateCouponRequest(string Code, decimal OrderTotal);
