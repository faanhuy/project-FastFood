using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Loyalty.Dtos;
using SmartShop.Application.Features.Loyalty.Queries.GetLoyaltyAccount;
using SmartShop.Application.Features.Loyalty.Queries.GetPointTransactions;
using SmartShop.Domain.Interfaces;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/loyalty")]
[Authorize]
public class LoyaltyController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get loyalty account info including tier and points balance</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<LoyaltyAccountDto>>> GetAccount(CancellationToken ct)
    {
        var result = await mediator.Send(new GetLoyaltyAccountQuery(CurrentUserId), ct);
        return Ok(ApiResponse<LoyaltyAccountDto>.Ok(result));
    }

    /// <summary>Get loyalty transaction history (paginated)</summary>
    [HttpGet("transactions")]
    public async Task<ActionResult<ApiResponse<PagedResult<PointTransactionDto>>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetPointTransactionsQuery(CurrentUserId, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<PointTransactionDto>>.Ok(result));
    }
}
