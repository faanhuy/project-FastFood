using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data.Seeders;

internal sealed class AppSettingsSeeder(
    ApplicationDbContext context,
    ILogger<AppSettingsSeeder> logger) : IDataSeeder
{
    public int Order => 1;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!await context.AppSettings.AnyAsync(cancellationToken))
        {
            var defaults = new[]
            {
                AppSetting.Create("AI:Search:MinScore",          "0.3",      "number", "Điểm tối thiểu để hiển thị kết quả tìm kiếm AI (0.0 - 1.0)"),
                AppSetting.Create("AI:Search:TopN",              "8",         "number", "Số kết quả tối đa trả về khi tìm kiếm AI"),
                AppSetting.Create("AI:Recommendations:Count",    "5",         "number", "Số sản phẩm gợi ý tối đa"),
                AppSetting.Create("AI:Recommendations:MinScore", "0.4",       "number", "Điểm tối thiểu để hiển thị gợi ý sản phẩm"),
                AppSetting.Create("Site:Name",                   "FastFood",  "text",   "Tên hiển thị của website"),
                AppSetting.Create("FileStorage:LocalBasePath",   "",          "text",   "Đường dẫn tuyệt đối lưu file upload (để trống = dùng wwwroot/uploads)"),
                AppSetting.Create("FileStorage:LocalUrlPrefix",  "/uploads",  "text",   "Prefix URL trả về cho file upload (ví dụ /images)"),
            };
            await context.AppSettings.AddRangeAsync(defaults, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} AppSettings.", defaults.Length);
            return;
        }

        var siteNameSetting = await context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "Site:Name", cancellationToken);
        if (siteNameSetting is not null && siteNameSetting.Value != "FastFood")
        {
            siteNameSetting.SetValue("FastFood");
            await context.SaveChangesAsync(cancellationToken);
        }

        // Đảm bảo các keys mới luôn tồn tại (upsert khi table đã có data)
        await EnsureKeyAsync("FileStorage:LocalBasePath", "",         "text", "Đường dẫn tuyệt đối lưu file upload (để trống = dùng wwwroot/uploads)", cancellationToken);
        await EnsureKeyAsync("FileStorage:LocalUrlPrefix", "/uploads", "text", "Prefix URL trả về cho file upload (ví dụ /images)", cancellationToken);
    }

    private async Task EnsureKeyAsync(string key, string defaultValue, string dataType, string description, CancellationToken ct)
    {
        var existing = await context.AppSettings.FindAsync([key], ct);
        if (existing is null)
        {
            context.AppSettings.Add(AppSetting.Create(key, defaultValue, dataType, description));
            await context.SaveChangesAsync(ct);
        }
    }
}
