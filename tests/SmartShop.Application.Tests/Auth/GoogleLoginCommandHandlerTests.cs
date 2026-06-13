using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Auth.Commands.GoogleLogin;
using SmartShop.Application.Features.Auth.Dtos;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Application.Tests.Auth;

public class GoogleLoginCommandHandlerTests
{
    private readonly Mock<IGoogleTokenValidator> _tokenValidator = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ITokenHasher> _tokenHasher = new();
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly Mock<IHttpContextAccessor> _httpContext = new();

    private GoogleLoginCommandHandler CreateHandler() =>
        new(_tokenValidator.Object, _userRepo.Object, _jwt.Object, _uow.Object,
            _tokenHasher.Object, _auditLog.Object, _httpContext.Object);

    private GoogleTokenValidationResult CreateValidPayload(
        string googleId = "google-123",
        string email = "test@google.com",
        string firstName = "John",
        string lastName = "Doe",
        bool emailVerified = true)
    {
        return new GoogleTokenValidationResult
        {
            GoogleUserId = googleId,
            Email = email,
            EmailVerified = emailVerified,
            FirstName = firstName,
            LastName = lastName
        };
    }

    [Fact]
    public async Task Handle_ValidTokenExistingGoogleUser_ReturnsAuthResponse()
    {
        // Arrange
        var payload = CreateValidPayload();
        var existingUser = User.CreateFromGoogle("google-123", "test@google.com", "John", "Doe");

        _tokenValidator.Setup(t => t.ValidateAsync("valid-token", default))
            .ReturnsAsync(payload);
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default))
            .ReturnsAsync(existingUser);
        _jwt.Setup(j => j.GenerateToken(existingUser)).Returns("jwt-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await CreateHandler().Handle(new GoogleLoginCommand("valid-token"), default);

        // Assert
        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be("test@google.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task Handle_ValidTokenNewUser_CreatesAndReturnsAuthResponse()
    {
        // Arrange
        var payload = CreateValidPayload();

        _tokenValidator.Setup(t => t.ValidateAsync("valid-token", default))
            .ReturnsAsync(payload);
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("test@google.com", default))
            .ReturnsAsync((User?)null);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await CreateHandler().Handle(new GoogleLoginCommand("valid-token"), default);

        // Assert
        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be("test@google.com");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsUnauthorizedException()
    {
        // Arrange
        _tokenValidator.Setup(t => t.ValidateAsync("invalid-token", default))
            .ReturnsAsync((GoogleTokenValidationResult?)null);

        // Act
        var act = () => CreateHandler().Handle(new GoogleLoginCommand("invalid-token"), default);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_EmailNotVerifiedByGoogle_ThrowsUnauthorizedException()
    {
        // Arrange
        var payload = CreateValidPayload(emailVerified: false);

        _tokenValidator.Setup(t => t.ValidateAsync("unverified-token", default))
            .ReturnsAsync(payload);

        // Act
        var act = () => CreateHandler().Handle(new GoogleLoginCommand("unverified-token"), default);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_EmailAlreadyExistsByTraditionalAccount_ThrowsConflictException()
    {
        // Arrange
        var payload = CreateValidPayload();
        var existingUser = User.Create("test@google.com", "hashed-password", "Jane", "Smith");

        _tokenValidator.Setup(t => t.ValidateAsync("valid-token", default))
            .ReturnsAsync(payload);
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("test@google.com", default))
            .ReturnsAsync(existingUser);

        // Act
        var act = () => CreateHandler().Handle(new GoogleLoginCommand("valid-token"), default);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_BannedUser_ThrowsConflictException()
    {
        // Arrange
        var payload = CreateValidPayload();
        var bannedUser = User.CreateFromGoogle("google-123", "test@google.com", "John", "Doe");
        bannedUser.Ban();

        _tokenValidator.Setup(t => t.ValidateAsync("valid-token", default))
            .ReturnsAsync(payload);
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default))
            .ReturnsAsync(bannedUser);

        // Act
        var act = () => CreateHandler().Handle(new GoogleLoginCommand("valid-token"), default);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ValidToken_SavesRefreshTokenHash()
    {
        // Arrange
        var payload = CreateValidPayload();
        var user = User.CreateFromGoogle("google-123", "test@google.com", "John", "Doe");

        _tokenValidator.Setup(t => t.ValidateAsync("valid-token", default))
            .ReturnsAsync(payload);
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default))
            .ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("jwt-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _tokenHasher.Setup(h => h.Hash("refresh-token")).Returns("hashed-refresh-token");
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await CreateHandler().Handle(new GoogleLoginCommand("valid-token"), default);

        // Assert
        user.RefreshTokenHash.Should().Be("hashed-refresh-token");
        user.RefreshToken.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NewUserCreation_UsesCorrectGoogleId()
    {
        // Arrange
        var payload = CreateValidPayload("unique-google-id-123");
        User? capturedUser = null;

        _tokenValidator.Setup(t => t.ValidateAsync("valid-token", default))
            .ReturnsAsync(payload);
        _userRepo.Setup(r => r.GetByGoogleIdAsync("unique-google-id-123", default))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("test@google.com", default))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await CreateHandler().Handle(new GoogleLoginCommand("valid-token"), default);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.GoogleId.Should().Be("unique-google-id-123");
        capturedUser.Email.Should().Be("test@google.com");
        capturedUser.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task Handle_ValidToken_CallsSaveChangesOnce()
    {
        // Arrange
        var payload = CreateValidPayload();
        var user = User.CreateFromGoogle("google-123", "test@google.com", "John", "Doe");

        _tokenValidator.Setup(t => t.ValidateAsync("valid-token", default))
            .ReturnsAsync(payload);
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default))
            .ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("jwt-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await CreateHandler().Handle(new GoogleLoginCommand("valid-token"), default);

        // Assert
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
