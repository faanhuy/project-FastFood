using FluentAssertions;
using Moq;
using Xunit;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Notifications.Commands.DeleteNotification;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Notifications;

public class DeleteNotificationCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly string UserIdString = UserId.ToString();

    public DeleteNotificationCommandHandlerTests()
    {
        _currentUser.Setup(s => s.UserId).Returns(UserIdString);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
    }

    private DeleteNotificationCommandHandler CreateHandler() =>
        new(_notificationRepo.Object, _uow.Object, _currentUser.Object);

    private static Notification CreateNotification(Guid userId) =>
        Notification.Create(userId, "Thong bao", "Noi dung thong bao");

    // ---------------------------------------------------------------
    // Delete single notification
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_SingleNotificationId_ValidOwner_ReturnsSuccess()
    {
        var notification = CreateNotification(UserId);
        var notificationId = notification.Id;

        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync(notification);

        var result = await CreateHandler().Handle(new DeleteNotificationCommand(notificationId), default);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SingleNotificationId_ValidOwner_DeletesAndSaves()
    {
        var notification = CreateNotification(UserId);
        var notificationId = notification.Id;

        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync(notification);

        await CreateHandler().Handle(new DeleteNotificationCommand(notificationId), default);

        _notificationRepo.Verify(r => r.DeleteAsync(notification, default), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_SingleNotificationId_NotFound_ThrowsNotFoundException()
    {
        var notificationId = Guid.NewGuid();
        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync((Notification?)null);

        var act = () => CreateHandler().Handle(new DeleteNotificationCommand(notificationId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_SingleNotificationId_DifferentOwner_ThrowsUnauthorizedException()
    {
        var otherUserId = Guid.NewGuid();
        var notification = CreateNotification(otherUserId);
        var notificationId = notification.Id;

        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync(notification);

        var act = () => CreateHandler().Handle(new DeleteNotificationCommand(notificationId), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_SingleNotificationId_DifferentOwner_DoesNotDeleteOrSave()
    {
        var otherUserId = Guid.NewGuid();
        var notification = CreateNotification(otherUserId);
        var notificationId = notification.Id;

        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync(notification);

        var act = () => CreateHandler().Handle(new DeleteNotificationCommand(notificationId), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _notificationRepo.Verify(r => r.DeleteAsync(It.IsAny<Notification>(), default), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    // ---------------------------------------------------------------
    // Delete all notifications (NotificationId = null)
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_NullNotificationId_DeletesAllAndSaves()
    {
        var result = await CreateHandler().Handle(new DeleteNotificationCommand(null), default);

        _notificationRepo.Verify(r => r.DeleteAllByUserIdAsync(UserId, default), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NullNotificationId_DoesNotCallSingleDelete()
    {
        await CreateHandler().Handle(new DeleteNotificationCommand(null), default);

        _notificationRepo.Verify(r => r.DeleteAsync(It.IsAny<Notification>(), default), Times.Never);
    }
}
