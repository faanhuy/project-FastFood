using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Common.Utilities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Infrastructure.Services;

public class LocalFileStorageService(
    IWebHostEnvironment env,
    IAppSettingRepository appSettingRepository,
    ILogger<LocalFileStorageService> logger
) : IFileStorageService
{
    private readonly string _fallbackBasePath = Path.Combine(env.WebRootPath, "uploads");
    private const string FallbackUrlPrefix = "/uploads";

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        UploadCategory category,
        CancellationToken ct = default)
    {
        var basePath  = await ResolveBasePathAsync(ct);
        var urlPrefix = await ResolveUrlPrefixAsync(ct);

        var config = UploadCategoryConfigProvider.Get(category);
        var categoryDir = Path.Combine(basePath, config.StoragePath);
        Directory.CreateDirectory(categoryDir);

        var safeFileName = FileSecurityHelper.SanitizeFileName(fileName);
        var filePath = Path.Combine(categoryDir, safeFileName);

        if (config.RequireImageOptimization)
        {
            await SaveOptimizedImageAsync(stream, filePath, config, ct);
        }
        else
        {
            await using var fileStream = System.IO.File.Create(filePath);
            await stream.CopyToAsync(fileStream, ct);
        }

        return $"{urlPrefix}/{config.StoragePath}/{safeFileName}";
    }

    public async Task DeleteAsync(string storedUrl, UploadCategory category, CancellationToken ct = default)
    {
        try
        {
            var basePath  = await ResolveBasePathAsync(ct);
            var urlPrefix = await ResolveUrlPrefixAsync(ct);

            // storedUrl là relative: /images/products/xxx.jpg
            var subPath = storedUrl.StartsWith(urlPrefix)
                ? storedUrl[urlPrefix.Length..].TrimStart('/')
                : storedUrl.TrimStart('/');
            var fullPath = Path.Combine(basePath, subPath.Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete file: {Url}", storedUrl);
        }
    }

    private async Task<string> ResolveBasePathAsync(CancellationToken ct)
    {
        var value = await appSettingRepository.GetStringAsync("FileStorage:LocalBasePath", "", ct);
        return string.IsNullOrWhiteSpace(value) ? _fallbackBasePath : value;
    }

    private async Task<string> ResolveUrlPrefixAsync(CancellationToken ct)
    {
        var value = await appSettingRepository.GetStringAsync("FileStorage:LocalUrlPrefix", "", ct);
        return string.IsNullOrWhiteSpace(value) ? FallbackUrlPrefix : value;
    }

    private static async Task SaveOptimizedImageAsync(
        Stream source,
        string filePath,
        UploadCategoryConfig config,
        CancellationToken ct)
    {
        source.Position = 0;
        using var image = await Image.LoadAsync(source, ct);

        if (config.ResizeWidth.HasValue)
        {
            var targetWidth = config.ResizeWidth.Value;
            var targetHeight = config.ResizeHeight ?? 0;

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(targetWidth, targetHeight),
                Mode = targetHeight > 0 ? ResizeMode.Crop : ResizeMode.Max
            }));
        }

        // Luôn save dưới dạng JPEG với quality 85 để tiết kiệm dung lượng
        await image.SaveAsJpegAsync(filePath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 85 }, ct);
    }
}
