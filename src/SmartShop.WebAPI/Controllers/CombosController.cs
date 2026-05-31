using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Combos;
using SmartShop.Application.Features.Combos.Commands.CreateCombo;
using SmartShop.Application.Features.Combos.Commands.DeleteCombo;
using SmartShop.Application.Features.Combos.Commands.UpdateCombo;
using SmartShop.Application.Features.Combos.Queries.GetComboById;
using SmartShop.Application.Features.Combos.Queries.GetCombos;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
public class CombosController(IMediator mediator) : ControllerBase
{
    /// <summary>Danh sách combo</summary>
    [HttpGet("api/admin/combos")]
    public async Task<ActionResult<ApiResponse<GetCombosResult>>> GetCombos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCombosQuery(page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Chi tiết combo</summary>
    [HttpGet("api/admin/combos/{id:guid}")]
    public async Task<ActionResult<ApiResponse<ComboDto>>> GetComboById(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetComboByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Danh sách combo đang active (public)</summary>
    [HttpGet("api/combos")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<GetCombosResult>>> GetPublicCombos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCombosQuery(page, pageSize), ct);
        if (result.Data is not null)
        {
            result.Data.Items.RemoveAll(c => !c.IsCurrentlyActive);
        }
        return Ok(result);
    }

    /// <summary>Chi tiết combo (public)</summary>
    [HttpGet("api/combos/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ComboDto>>> GetPublicComboById(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetComboByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Tạo combo mới</summary>
    [HttpPost("api/admin/combos")]
    public async Task<ActionResult<ApiResponse<ComboDto>>> CreateCombo(
        [FromBody] CreateComboCommand command,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Cập nhật combo</summary>
    [HttpPut("api/admin/combos/{id:guid}")]
    public async Task<ActionResult<ApiResponse<ComboDto>>> UpdateCombo(
        Guid id,
        [FromBody] UpdateComboCommand command,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return Ok(result);
    }

    /// <summary>Vô hiệu hóa combo</summary>
    [HttpDelete("api/admin/combos/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteCombo(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new DeleteComboCommand(id), ct);
        return Ok(result);
    }
}
