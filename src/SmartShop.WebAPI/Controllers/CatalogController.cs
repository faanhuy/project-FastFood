using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Catalog;
using SmartShop.Application.Features.Catalog.Queries.GetCatalog;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController(IMediator mediator) : ControllerBase
{
    /// <summary>Lấy danh sách catalog gộp: products + combos active</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<GetCatalogResult>>> GetCatalog(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCatalogQuery(page, pageSize), ct);
        return Ok(result);
    }
}
