using FluentAssertions;
using SmartShop.Application.Common.Utilities;
using Xunit;

namespace SmartShop.Application.Tests.Files;

public class FileSecurityHelperTests
{
    #region ValidateMagicBytes Tests

    [Fact]
    public void ValidateMagicBytes_JpegFile_ReturnsTrue()
    {
        // Arrange
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        var stream = new MemoryStream(jpegBytes);
        var allowedMimeTypes = new[] { "image/jpeg" };

        // Act
        var result = FileSecurityHelper.ValidateMagicBytes(stream, allowedMimeTypes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateMagicBytes_PngFile_ReturnsTrue()
    {
        // Arrange
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var stream = new MemoryStream(pngBytes);
        var allowedMimeTypes = new[] { "image/png" };

        // Act
        var result = FileSecurityHelper.ValidateMagicBytes(stream, allowedMimeTypes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateMagicBytes_WebpFile_ReturnsTrue()
    {
        // Arrange
        var webpBytes = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00 };
        var stream = new MemoryStream(webpBytes);
        var allowedMimeTypes = new[] { "image/webp" };

        // Act
        var result = FileSecurityHelper.ValidateMagicBytes(stream, allowedMimeTypes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateMagicBytes_WrongBytes_ReturnsFalse()
    {
        // Arrange
        var wrongBytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
        var stream = new MemoryStream(wrongBytes);
        var allowedMimeTypes = new[] { "image/jpeg" };

        // Act
        var result = FileSecurityHelper.ValidateMagicBytes(stream, allowedMimeTypes);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateMagicBytes_TooShortStream_ReturnsFalse()
    {
        // Arrange
        var shortBytes = new byte[] { 0x00, 0x01 };
        var stream = new MemoryStream(shortBytes);
        var allowedMimeTypes = new[] { "image/jpeg" };

        // Act
        var result = FileSecurityHelper.ValidateMagicBytes(stream, allowedMimeTypes);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateMagicBytes_CsvMimeNoSignature_ReturnsTrue()
    {
        // Arrange
        var csvBytes = new byte[] { 0x00, 0x01, 0x02 };
        var stream = new MemoryStream(csvBytes);
        var allowedMimeTypes = new[] { "text/csv" };

        // Act - CSV has no magic byte signature, so it returns true (bypass validation)
        var result = FileSecurityHelper.ValidateMagicBytes(stream, allowedMimeTypes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateMagicBytes_StreamPositionResetAfterValidation()
    {
        // Arrange
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        var stream = new MemoryStream(jpegBytes);
        var allowedMimeTypes = new[] { "image/jpeg" };

        // Act
        FileSecurityHelper.ValidateMagicBytes(stream, allowedMimeTypes);

        // Assert
        stream.Position.Should().Be(0);
    }

    #endregion

    #region SanitizeFileName Tests

    [Fact]
    public void SanitizeFileName_PathTraversal_RemovesPath()
    {
        // Arrange
        var maliciousPath = "../../etc/passwd";

        // Act
        var result = FileSecurityHelper.SanitizeFileName(maliciousPath);

        // Assert
        result.Should().Be("passwd");
    }

    [Fact]
    public void SanitizeFileName_SpacesRemoved()
    {
        // Arrange
        var fileName = "my file.jpg";

        // Act
        var result = FileSecurityHelper.SanitizeFileName(fileName);

        // Assert
        result.Should().Be("myfile.jpg");
    }

    [Fact]
    public void SanitizeFileName_LongName_Truncated()
    {
        // Arrange
        var longFileName = new string('a', 200) + ".jpg";

        // Act
        var result = FileSecurityHelper.SanitizeFileName(longFileName);

        // Assert
        result.Length.Should().Be(100);
    }

    [Fact]
    public void SanitizeFileName_ValidFileName_Unchanged()
    {
        // Arrange
        var fileName = "product.jpg";

        // Act
        var result = FileSecurityHelper.SanitizeFileName(fileName);

        // Assert
        result.Should().Be("product.jpg");
    }

    #endregion

    #region BuildStorageFileName Tests

    [Fact]
    public void BuildStorageFileName_HasExtension_ContainsExt()
    {
        // Arrange
        var originalName = "product.jpg";

        // Act
        var result = FileSecurityHelper.BuildStorageFileName(originalName);

        // Assert
        result.Should().EndWith(".jpg");
    }

    [Fact]
    public void BuildStorageFileName_NoExtension_UsesBin()
    {
        // Arrange
        var originalName = "productfile";

        // Act
        var result = FileSecurityHelper.BuildStorageFileName(originalName);

        // Assert
        result.Should().EndWith(".bin");
    }

    [Fact]
    public void BuildStorageFileName_WithEntityId_ContainsPrefix()
    {
        // Arrange
        var originalName = "product.jpg";
        var entityId = "abcd1234";

        // Act
        var result = FileSecurityHelper.BuildStorageFileName(originalName, entityId);

        // Assert
        result.Should().StartWith("abcd");
    }

    [Fact]
    public void BuildStorageFileName_WithoutEntityId_StartsWithRandom()
    {
        // Arrange
        var originalName = "product.jpg";

        // Act
        var result = FileSecurityHelper.BuildStorageFileName(originalName);

        // Assert
        var parts = result.Split('-');
        parts.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void BuildStorageFileName_LongEntityId_Truncates()
    {
        // Arrange
        var originalName = "product.jpg";
        var longEntityId = "abcdefghijklmnopqrst";

        // Act
        var result = FileSecurityHelper.BuildStorageFileName(originalName, longEntityId);

        // Assert
        result.Should().StartWith("abcdefgh");
    }

    #endregion
}
