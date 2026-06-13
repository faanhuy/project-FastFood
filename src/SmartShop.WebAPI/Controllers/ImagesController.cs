using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/images")]
[Authorize(Roles = "Admin")]
public class ImagesController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    /// <summary>Upload hình ảnh sản phẩm — trả về đường dẫn tương đối</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<UploadImageResponse>>> Upload(
        IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("Please select an image file."));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(ApiResponse.Fail($"Only accepted: {string.Join(", ", AllowedExtensions)}"));

        if (file.Length > MaxBytes)
            return BadRequest(ApiResponse.Fail("File size must not exceed 5 MB."));

        // Lưu vào wwwroot/images/products/{guid}.{ext}
        var uploadDir = Path.Combine(env.WebRootPath, "images", "products");
        Directory.CreateDirectory(uploadDir); // idempotent — đảm bảo tồn tại

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        var relativeUrl = $"/images/products/{fileName}";
        return Ok(ApiResponse<UploadImageResponse>.Ok(new UploadImageResponse(relativeUrl)));
    }
}

public record UploadImageResponse(string Url);
