using FluentAssertions;
using Moq;
using Xunit;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Notifications.Queries.GetNotifications;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Notifications;

public class GetNotificationsQueryHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private static readonly string UserId = Guid.NewGuid().ToString();

    public GetNotificationsQueryHandlerTests()
    {
        _currentUser.Setup(s => s.UserId).Returns(UserId);
    }

    private GetNotificationsQueryHandler CreateHandler() =>
        new(_notificationRepo.Object, _currentUser.Object);

    private static Notification CreateNotification(string userId, bool isRead = false)
    {
        var notification = Notification.Create(userId, "Thong bao test", "Noi dung test");
        if (isRead) notification.MarkAsRead();
        return notification;
    }

    [Fact]
    public async Task Handle_NoNotifications_ReturnsEmptyList()
    {
        _notificationRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                         .ReturnsAsync(new List<Notification>());

        var result = await CreateHandler().Handle(new GetNotificationsQuery(), default);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNotifications_ReturnsDtoList()
    {
        var n1 = CreateNotification(UserId);
        var n2 = CreateNotification(UserId, isRead: true);

        _notificationRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                         .ReturnsAsync(new List<Notification> { n1, n2 });

        var result = await CreateHandler().Handle(new GetNotificationsQuery(), default);

        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNotifications_MapsDtoFieldsCorrectly()
    {
        var notification = CreateNotification(UserId);
        var orderId = Guid.NewGuid();
        var notificationWithOrder = Notification.Create(UserId, "Don hang moi", "Don hang cua ban da duoc dat", orderId);

        _notificationRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                         .ReturnsAsync(new List<Notification> { notificationWithOrder });

        var result = await CreateHandler().Handle(new GetNotificationsQuery(), default);

        var dto = result.Data!.Single();
        dto.Title.Should().Be("Don hang moi");
        dto.Message.Should().Be("Don hang cua ban da duoc dat");
        dto.IsRead.Should().BeFalse();
        dto.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task Handle_ReadStatusPreservedInDto()
    {
        var unreadNotification = CreateNotification(UserId, isRead: false);
        var readNotification = CreateNotification(UserId, isRead: true);

        _notificationRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                         .ReturnsAsync(new List<Notification> { unreadNotification, readNotification });

        var result = await CreateHandler().Handle(new GetNotificationsQuery(), default);

        result.Data!.Should().ContainSingle(d => !d.IsRead);
        result.Data!.Should().ContainSingle(d => d.IsRead);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCurrentUserId()
    {
        _notificationRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                         .ReturnsAsync(new List<Notification>());

        await CreateHandler().Handle(new GetNotificationsQuery(), default);

        _notificationRepo.Verify(r => r.GetByUserIdAsync(UserId, default), Times.Once);
    }
}
