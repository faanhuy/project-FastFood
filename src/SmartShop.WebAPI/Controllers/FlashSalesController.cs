using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Application.Features.FlashSales.Queries.GetActiveFlashSales;
using SmartShop.Domain.Interfaces;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/flash-sales")]
[AllowAnonymous]
public class FlashSalesController(IMediator mediator) : ControllerBase
{
    /// <summary>Get active flash sales (public, paginated)</summary>
    [HttpGet]
    public async Task<IActionResult> GetActive(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetActiveFlashSalesQuery(page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<FlashSaleDto>>.Ok(result));
    }
}
