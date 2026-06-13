using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Application.Features.FlashSales.Commands.ApproveFlashSale;
using SmartShop.Application.Features.FlashSales.Commands.CreateFlashSale;
using SmartShop.Application.Features.FlashSales.Commands.DeleteFlashSale;
using SmartShop.Application.Features.FlashSales.Commands.RejectFlashSale;
using SmartShop.Application.Features.FlashSales.Commands.SubmitFlashSaleForApproval;
using SmartShop.Application.Features.FlashSales.Commands.UpdateFlashSale;
using SmartShop.Application.Features.FlashSales.Queries.GetActiveFlashSales;
using SmartShop.Application.Features.FlashSales.Queries.GetFlashSalesAdmin;
using SmartShop.Domain.Interfaces;
using CreateFlashSaleRequest = SmartShop.Application.Features.FlashSales.CreateFlashSaleRequest;
using UpdateFlashSaleRequest = SmartShop.Application.Features.FlashSales.UpdateFlashSaleRequest;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/admin/flash-sales")]
[Authorize(Roles = "Admin")]
public class AdminFlashSalesController(IMediator mediator) : ControllerBase
{
    /// <summary>Get all flash sales (admin, paginated with filter)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetFlashSalesAdminQuery(page, pageSize, isActive, status), ct);
        return Ok(ApiResponse<PagedResult<FlashSaleDto>>.Ok(result));
    }

    /// <summary>Create new flash sale</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateFlashSaleRequest request,
        CancellationToken ct = default)
    {
        var command = new CreateFlashSaleCommand(
            request.Name,
            request.StartAt,
            request.EndAt,
            request.Items);

        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), ApiResponse<FlashSaleDto>.Ok(result));
    }

    /// <summary>Update flash sale</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateFlashSaleRequest request,
        CancellationToken ct = default)
    {
        var command = new UpdateFlashSaleCommand(
            id,
            request.Name,
            request.StartAt,
            request.EndAt,
            request.Items);

        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<FlashSaleDto>.Ok(result));
    }

    /// <summary>Delete flash sale</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await mediator.Send(new DeleteFlashSaleCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new object(), "Flash sale deleted successfully."));
    }

    /// <summary>Submit flash sale for approval</summary>
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new SubmitFlashSaleForApprovalCommand(id), ct);
        return Ok(ApiResponse<FlashSaleDto>.Ok(result));
    }

    /// <summary>Approve flash sale</summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct = default)
    {
        var approvedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "admin";
        var result = await mediator.Send(new ApproveFlashSaleCommand(id, approvedBy), ct);
        return Ok(ApiResponse<FlashSaleDto>.Ok(result));
    }

    /// <summary>Reject flash sale</summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectFlashSaleRequest request, CancellationToken ct = default)
    {
        var result = await mediator.Send(new RejectFlashSaleCommand(id, request.Reason), ct);
        return Ok(ApiResponse<FlashSaleDto>.Ok(result));
    }
}

public record RejectFlashSaleRequest(string Reason);

