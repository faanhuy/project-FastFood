using FluentAssertions;
using Moq;
using Xunit;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Notifications.Commands.MarkAsRead;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Notifications;

public class MarkAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private static readonly string UserId = Guid.NewGuid().ToString();

    public MarkAsReadCommandHandlerTests()
    {
        _currentUser.Setup(s => s.UserId).Returns(UserId);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
    }

    private MarkAsReadCommandHandler CreateHandler() =>
        new(_notificationRepo.Object, _uow.Object, _currentUser.Object);

    private static Notification CreateNotification(string userId, bool isRead = false)
    {
        var notification = Notification.Create(userId, "Thong bao", "Noi dung thong bao");
        if (isRead) notification.MarkAsRead();
        return notification;
    }

    // ---------------------------------------------------------------
    // Mark single notification as read
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_SingleNotificationId_ValidOwner_ReturnsSuccess()
    {
        var notification = CreateNotification(UserId);
        var notificationId = notification.Id;

        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync(notification);

        var result = await CreateHandler().Handle(new MarkAsReadCommand(notificationId), default);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SingleNotificationId_ValidOwner_MarksAsReadAndSaves()
    {
        var notification = CreateNotification(UserId);
        var notificationId = notification.Id;

        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync(notification);

        await CreateHandler().Handle(new MarkAsReadCommand(notificationId), default);

        notification.IsRead.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_SingleNotificationId_NotFound_ThrowsNotFoundException()
    {
        var notificationId = Guid.NewGuid();
        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync((Notification?)null);

        var act = () => CreateHandler().Handle(new MarkAsReadCommand(notificationId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_SingleNotificationId_DifferentOwner_ThrowsUnauthorizedException()
    {
        var otherUserId = Guid.NewGuid().ToString();
        var notification = CreateNotification(otherUserId);
        var notificationId = notification.Id;

        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync(notification);

        var act = () => CreateHandler().Handle(new MarkAsReadCommand(notificationId), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_SingleNotificationId_DifferentOwner_DoesNotSave()
    {
        var otherUserId = Guid.NewGuid().ToString();
        var notification = CreateNotification(otherUserId);
        var notificationId = notification.Id;

        _notificationRepo.Setup(r => r.GetByIdAsync(notificationId, default))
                         .ReturnsAsync(notification);

        var act = () => CreateHandler().Handle(new MarkAsReadCommand(notificationId), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    // ---------------------------------------------------------------
    // Mark all notifications as read (NotificationId = null)
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_NullNotificationId_MarksAllUnreadAsRead()
    {
        var unread1 = CreateNotification(UserId);
        var unread2 = CreateNotification(UserId);
        var alreadyRead = CreateNotification(UserId, isRead: true);

        _notificationRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                         .ReturnsAsync(new List<Notification> { unread1, unread2, alreadyRead });

        await CreateHandler().Handle(new MarkAsReadCommand(null), default);

        unread1.IsRead.Should().BeTrue();
        unread2.IsRead.Should().BeTrue();
        alreadyRead.IsRead.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_NullNotificationId_EmptyList_StillSaves()
    {
        _notificationRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                         .ReturnsAsync(new List<Notification>());

        var result = await CreateHandler().Handle(new MarkAsReadCommand(null), default);

        result.Success.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_NullNotificationId_AllAlreadyRead_SavesOnce()
    {
        var read1 = CreateNotification(UserId, isRead: true);
        var read2 = CreateNotification(UserId, isRead: true);

        _notificationRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                         .ReturnsAsync(new List<Notification> { read1, read2 });

        await CreateHandler().Handle(new MarkAsReadCommand(null), default);

        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
