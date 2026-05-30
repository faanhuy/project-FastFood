using FluentAssertions;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using Xunit;

namespace SmartShop.Application.Tests.Domain;

public class LocalizationEntityTests
{
    [Fact]
    public void LocalizedMessage_Create_SetsProperties()
    {
        var msg = LocalizedMessage.Create("error.not_found", "vi", "Không tìm thấy");
        msg.MessageKey.Should().Be("error.not_found");
        msg.Language.Should().Be("vi");
        msg.MessageText.Should().Be("Không tìm thấy");
        msg.Category.Should().Be("error");
    }

    [Fact]
    public void LocalizedMessage_Create_WithCategory_SetsCategory()
    {
        var msg = LocalizedMessage.Create("success.created", "en", "Created successfully", "success");
        msg.Category.Should().Be("success");
    }

    [Fact]
    public void LocalizedMessage_Create_EmptyKey_ThrowsArgumentException()
    {
        var act = () => LocalizedMessage.Create("", "vi", "text");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LocalizedFieldName_Create_SetsProperties()
    {
        var field = LocalizedFieldName.Create("user.email", "vi", "Email");
        field.FieldKey.Should().Be("user.email");
        field.Language.Should().Be("vi");
        field.DisplayName.Should().Be("Email");
    }

    [Fact]
    public void LocalizedFieldName_Create_EmptyKey_ThrowsArgumentException()
    {
        var act = () => LocalizedFieldName.Create("", "vi", "Email");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConflictException_WithMessageKey_SetsProperties()
    {
        var ex = new ConflictException("error.combo_inactive", null);
        ex.MessageKey.Should().Be("error.combo_inactive");
        ex.Params.Should().BeNull();
        ex.Message.Should().Be("error.combo_inactive");
    }

    [Fact]
    public void ConflictException_WithStringMessage_SetsMessage()
    {
        var ex = new ConflictException("Có lỗi xảy ra");
        ex.Message.Should().Be("Có lỗi xảy ra");
        ex.MessageKey.Should().BeNull();
    }

    [Fact]
    public void UnauthorizedException_WithMessageKey_SetsProperties()
    {
        var ex = new UnauthorizedException("error.unauthorized", null);
        ex.MessageKey.Should().Be("error.unauthorized");
        ex.Params.Should().BeNull();
        ex.Message.Should().Be("error.unauthorized");
    }

    [Fact]
    public void UnauthorizedException_WithStringMessage_SetsMessage()
    {
        var ex = new UnauthorizedException("Không có quyền");
        ex.Message.Should().Be("Không có quyền");
        ex.MessageKey.Should().BeNull();
    }
}
