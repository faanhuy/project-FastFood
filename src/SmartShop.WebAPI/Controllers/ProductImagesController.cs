using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Products.Commands.AddProductImage;
using SmartShop.Application.Features.Products.Commands.DeleteProductImage;
using SmartShop.Application.Features.Products.Commands.SetPrimaryImage;
using SmartShop.Application.Features.Products.Queries.GetProductImages;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api")]
public class ProductImagesController(IMediator mediator) : ControllerBase
{
    [HttpGet("products/{productId}/images")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImages(Guid productId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductImagesQuery(productId), ct);
        return Ok(ApiResponse<List<ProductImageDto>>.Ok(result));
    }

    [HttpPost("admin/products/{productId}/images")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddImage(Guid productId, [FromBody] AddProductImageRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddProductImageCommand(productId, request.ImageUrl, request.IsPrimary, request.SortOrder), ct);
        return Ok(ApiResponse<ProductImageDto>.Ok(result));
    }

    [HttpDelete("admin/products/{productId}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(Guid productId, Guid imageId, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductImageCommand(productId, imageId), ct);
        return Ok(ApiResponse<string>.Ok("Deleted"));
    }

    [HttpPatch("admin/products/{productId}/images/{imageId}/primary")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetPrimary(Guid productId, Guid imageId, CancellationToken ct)
    {
        await mediator.Send(new SetPrimaryImageCommand(productId, imageId), ct);
        return Ok(ApiResponse<string>.Ok("Updated"));
    }
}

public record AddProductImageRequest(string ImageUrl, bool IsPrimary = false, int SortOrder = 0);
