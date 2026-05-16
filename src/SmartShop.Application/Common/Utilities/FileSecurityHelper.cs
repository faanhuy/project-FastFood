namespace SmartShop.Application.Common.Utilities;

public static class FileSecurityHelper
{
    // Magic byte signatures cho từng MIME type
    private static readonly Dictionary<string, byte[]> MagicByteMap = new()
    {
        ["image/jpeg"] = [0xFF, 0xD8, 0xFF],
        ["image/png"]  = [0x89, 0x50, 0x4E, 0x47],
        ["image/webp"] = [0x52, 0x49, 0x46, 0x46],  // "RIFF" — WebP bắt đầu bằng RIFF
    };

    /// <summary>
    /// Validate MIME type dựa trên magic bytes của file.
    /// Stream position reset về 0 trước và sau khi đọc.
    /// </summary>
    public static bool ValidateMagicBytes(Stream stream, string[] allowedMimeTypes)
    {
        stream.Position = 0;
        var buffer = new byte[8];
        var read = stream.Read(buffer, 0, buffer.Length);
        stream.Position = 0;

        if (read < 3) return false;

        foreach (var mime in allowedMimeTypes)
        {
            if (!MagicByteMap.TryGetValue(mime, out var signature))
                return true; // MIME không có signature → bỏ qua (CSV, etc.)

            if (buffer.Take(signature.Length).SequenceEqual(signature))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Sanitize file name — chỉ lấy tên file (không path), loại ký tự đặc biệt, giới hạn 100 ký tự.
    /// </summary>
    public static string SanitizeFileName(string rawFileName)
    {
        var name = Path.GetFileName(rawFileName);
        var invalidChars = Path.GetInvalidFileNameChars();
        name = new string(name.Where(c => !invalidChars.Contains(c) && c != ' ').ToArray());
        return name.Length > 100 ? name[^100..] : name;
    }

    /// <summary>Build file name an toàn: {entityId}-{timestamp}-{random}.{ext}</summary>
    public static string BuildStorageFileName(string originalName, string? entityId = null)
    {
        var ext = Path.GetExtension(originalName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext)) ext = ".bin";

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..6];
        var prefix = string.IsNullOrEmpty(entityId) ? random : $"{entityId[..Math.Min(8, entityId.Length)]}-{random}";

        return $"{prefix}-{timestamp}{ext}";
    }
}
