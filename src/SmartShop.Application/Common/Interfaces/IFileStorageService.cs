using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>Upload file → return stored URL (relative cho local, absolute cho cloud)</summary>
    Task<string> UploadAsync(Stream stream, string fileName, UploadCategory category, CancellationToken ct = default);

    /// <summary>Delete stored file. Silent nếu file không tồn tại.</summary>
    Task DeleteAsync(string storedUrl, UploadCategory category, CancellationToken ct = default);
}
