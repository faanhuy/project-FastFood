using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.AuditLogs;
using SmartShop.Application.Features.AuditLogs.Queries.GetAuditLogs;
using SmartShop.Application.Products.Queries.GetProducts;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = new GetAuditLogsQuery(userId, action, entityType, fromDate, toDate, page, pageSize);
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }
}
