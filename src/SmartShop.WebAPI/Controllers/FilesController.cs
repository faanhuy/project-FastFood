using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Combos.Commands.UploadComboImage;
using SmartShop.Application.Features.Products.Commands.UploadProductImage;
using SmartShop.Application.Features.Reviews.Commands.AddReviewImages;
using SmartShop.Application.Features.Users.Commands.UploadAvatar;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController(IMediator mediator) : ControllerBase
{
    [HttpPost("upload/product-image")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<UploadProductImageResult>>> UploadProductImage(
        IFormFile file,
        CancellationToken ct)
    {
        var command = new UploadProductImageCommand(file);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<UploadProductImageResult>.Ok(result));
    }

    [HttpPost("upload/combo-image")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<UploadComboImageResult>>> UploadComboImage(
        IFormFile file,
        CancellationToken ct)
    {
        var command = new UploadComboImageCommand(file);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<UploadComboImageResult>.Ok(result));
    }

    [HttpPost("upload/avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<UploadUserAvatarResult>>> UploadAvatar(
        IFormFile file,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new UploadUserAvatarCommand(userId, file);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<UploadUserAvatarResult>.Ok(result));
    }

    [HttpPost("upload/review-images")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<AddReviewImagesResult>>> UploadReviewImages(
        [FromQuery] Guid reviewId,
        IList<IFormFile> files,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new AddReviewImagesCommand(reviewId, userId, files);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<AddReviewImagesResult>.Ok(result));
    }
}
