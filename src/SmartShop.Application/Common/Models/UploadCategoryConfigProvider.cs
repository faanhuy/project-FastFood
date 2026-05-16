namespace SmartShop.Application.Common.Models;

public static class UploadCategoryConfigProvider
{
    private static readonly Dictionary<UploadCategory, UploadCategoryConfig> Configs = new()
    {
        [UploadCategory.ProductImage] = new(
            Category: UploadCategory.ProductImage,
            MaxSizeBytes: 5 * 1024 * 1024,
            AllowedMimeTypes: ["image/jpeg", "image/png", "image/webp"],
            StoragePath: "products",
            RequireImageOptimization: true,
            ResizeWidth: 800,
            ResizeHeight: 800
        ),
        [UploadCategory.ComboImage] = new(
            Category: UploadCategory.ComboImage,
            MaxSizeBytes: 5 * 1024 * 1024,
            AllowedMimeTypes: ["image/jpeg", "image/png", "image/webp"],
            StoragePath: "combos",
            RequireImageOptimization: true,
            ResizeWidth: 600,
            ResizeHeight: 600
        ),
        [UploadCategory.UserAvatar] = new(
            Category: UploadCategory.UserAvatar,
            MaxSizeBytes: 2 * 1024 * 1024,
            AllowedMimeTypes: ["image/jpeg", "image/png", "image/webp"],
            StoragePath: "avatars",
            RequireImageOptimization: true,
            ResizeWidth: 200,
            ResizeHeight: 200
        ),
        [UploadCategory.ReviewImage] = new(
            Category: UploadCategory.ReviewImage,
            MaxSizeBytes: 3 * 1024 * 1024,
            AllowedMimeTypes: ["image/jpeg", "image/png", "image/webp"],
            StoragePath: "reviews",
            RequireImageOptimization: true,
            ResizeWidth: 400,
            ResizeHeight: null  // Giữ aspect ratio
        ),
        [UploadCategory.CsvImport] = new(
            Category: UploadCategory.CsvImport,
            MaxSizeBytes: 10 * 1024 * 1024,
            AllowedMimeTypes: ["text/csv", "application/vnd.ms-excel", "text/plain"],
            StoragePath: "imports",
            RequireImageOptimization: false
        )
    };

    public static UploadCategoryConfig Get(UploadCategory category)
        => Configs.TryGetValue(category, out var config)
            ? config
            : throw new ArgumentOutOfRangeException(nameof(category), $"No config for category: {category}");
}
