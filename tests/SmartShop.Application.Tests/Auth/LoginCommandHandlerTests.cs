using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Auth.Commands.Login;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ITokenHasher> _tokenHasher = new();
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly Mock<IHttpContextAccessor> _httpContext = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepo.Object, _hasher.Object, _jwt.Object, _uow.Object,
            _tokenHasher.Object, _auditLog.Object, _httpContext.Object);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        var user = User.Create("test@test.com", "hashed", "John", "Doe");
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("password", "hashed")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("jwt-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(new LoginCommand("test@test.com", "password"), default);

        result.Token.Should().Be("jwt-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);

        var act = () => CreateHandler().Handle(new LoginCommand("unknown@test.com", "password"), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = User.Create("test@test.com", "hashed", "John", "Doe");
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrongpassword", "hashed")).Returns(false);

        var act = () => CreateHandler().Handle(new LoginCommand("test@test.com", "wrongpassword"), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ValidCredentials_CallsSaveChangesOnce()
    {
        var user = User.Create("test@test.com", "hashed", "John", "Doe");
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("password", "hashed")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("jwt-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await CreateHandler().Handle(new LoginCommand("test@test.com", "password"), default);

        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCredentials_SetsHashedRefreshTokenOnUser()
    {
        var user = User.Create("test@test.com", "hashed", "John", "Doe");
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("password", "hashed")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("jwt-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _tokenHasher.Setup(h => h.Hash("refresh-token")).Returns("hashed-refresh-token");
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await CreateHandler().Handle(new LoginCommand("test@test.com", "password"), default);

        // Sprint 23: refresh token được hash trước khi lưu — plaintext không persist.
        user.RefreshTokenHash.Should().Be("hashed-refresh-token");
        user.RefreshToken.Should().BeNull();
    }
}
