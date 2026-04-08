using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Categories;
using SmartShop.Application.Features.Categories.Queries.GetCategories;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    /// <summary>Lấy danh sách categories đang hoạt động</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoriesQuery(), ct);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(result));
    }
}
