using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.DTOs;
using SmartShop.Application.Features.Products.Commands.BulkImportProducts;
using SmartShop.Application.Products.Commands.CreateProduct;
using SmartShop.Application.Products.Commands.DeleteProduct;
using SmartShop.Application.Products.Commands.UpdateProduct;
using SmartShop.Application.Products.Queries.GetProductById;
using SmartShop.Application.Products.Queries.GetProductBySlug;
using SmartShop.Application.Products.Queries.GetProducts;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>Lấy danh sách sản phẩm (phân trang, lọc, tìm kiếm, sắp xếp)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] ProductSortBy sortBy = ProductSortBy.Newest,
        [FromQuery] Guid? storeId = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductsQuery(page, pageSize, categoryId, search, sortBy, storeId), ct);
        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
    }

    /// <summary>Lấy chi tiết một sản phẩm theo Id</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetById(
        Guid id,
        [FromQuery] Guid? storeId = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id, storeId), ct);
        return Ok(ApiResponse<ProductDetailDto>.Ok(result));
    }

    /// <summary>Lấy chi tiết một sản phẩm theo Slug</summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetBySlug(
        string slug,
        [FromQuery] Guid? storeId = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductBySlugQuery(slug, storeId), ct);
        return Ok(ApiResponse<ProductDetailDto>.Ok(result));
    }

    /// <summary>Tạo sản phẩm mới (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProductDto>.Ok(result));
    }

    /// <summary>Cập nhật sản phẩm (Admin only)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.ImageUrl, request.OriginalPrice, request.HasSizes, request.SizeType), ct);
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }

    /// <summary>Xoá mềm sản phẩm (Admin only)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);
        return Ok(ApiResponse.Ok("Sản phẩm đã được xoá."));
    }

    /// <summary>Nhập sản phẩm từ file CSV (Admin only)</summary>
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<BulkImportProductsResult>>> BulkImportProducts(
        IFormFile file,
        CancellationToken ct)
    {
        var command = new BulkImportProductsCommand(file);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<BulkImportProductsResult>.Ok(result));
    }
}

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    decimal? OriginalPrice = null,
    bool HasSizes = false,
    SmartShop.Domain.Enums.SizeType? SizeType = null
);
