namespace SmartShop.Application.Common.Models;

public record UploadCategoryConfig(
    UploadCategory Category,
    long MaxSizeBytes,
    string[] AllowedMimeTypes,
    string StoragePath,
    bool RequireImageOptimization,
    int? ResizeWidth = null,
    int? ResizeHeight = null
);
