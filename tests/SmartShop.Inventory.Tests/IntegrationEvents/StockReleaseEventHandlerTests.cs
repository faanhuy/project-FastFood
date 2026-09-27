using System.Text.Json;
using FluentAssertions;
using MediatR;
using Moq;
using SmartShop.Contracts.Events;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;
using SmartShop.Inventory.Application.IntegrationEvents;
using Xunit;

namespace SmartShop.Inventory.Tests.IntegrationEvents;

public class StockReleaseEventHandlerTests
{
    private readonly Mock<ISender> _sender = new();

    private StockReleaseEventHandler CreateHandler() => new(_sender.Object);

    // Core ghi payload vào Outbox bằng JsonSerializer.Serialize mặc định (PascalCase) —
    // test dựng payload đúng cách đó để bắt được lệch định dạng giữa 2 service.
    private static string CancelledPayload(Guid orderId) =>
        JsonSerializer.Serialize(new OrderCancelledIntegrationEvent(
            orderId, Guid.NewGuid(), "user@example.com", "Test User", DateTime.UtcNow));

    private static string PaymentFailedPayload(Guid orderId) =>
        JsonSerializer.Serialize(new PaymentFailedIntegrationEvent(
            orderId, Guid.NewGuid(), "user@example.com", "Test User", "Card declined", DateTime.UtcNow));

    private void VerifyReleased(Guid orderId) =>
        _sender.Verify(
            s => s.Send(It.Is<ReleaseStockCommand>(c => c.OrderId == orderId), It.IsAny<CancellationToken>()),
            Times.Once());

    private void VerifyNothingReleased() =>
        _sender.Verify(
            s => s.Send(It.IsAny<ReleaseStockCommand>(), It.IsAny<CancellationToken>()),
            Times.Never());

    [Fact]
    public async Task HandleAsync_OrderCancelled_ReleasesStockForThatOrder()
    {
        var orderId = Guid.NewGuid();

        await CreateHandler().HandleAsync(EventTopics.OrderCancelled, CancelledPayload(orderId), default);

        VerifyReleased(orderId);
    }

    [Fact]
    public async Task HandleAsync_PaymentFailed_IsNotAReleaseTrigger_BecauseTheOrderCanStillBePaid()
    {
        // Core không hủy đơn khi thanh toán fail và cho thanh toán lại; nhả chỗ ở đây sẽ làm đơn được trả tiền sau đó mất chỗ
        var act = () => CreateHandler().HandleAsync(
            EventTopics.PaymentFailed, PaymentFailedPayload(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<InvalidIntegrationEventException>();
        VerifyNothingReleased();
    }

    [Fact]
    public async Task HandleAsync_CamelCasePayload_IsAlsoAccepted()
    {
        var orderId = Guid.NewGuid();
        var payload = $$"""{"orderId":"{{orderId}}"}""";

        await CreateHandler().HandleAsync(EventTopics.OrderCancelled, payload, default);

        VerifyReleased(orderId);
    }

    [Theory]
    [InlineData("smartshop.order.placed")]
    [InlineData("smartshop.payment.completed")]
    [InlineData("some.other.topic")]
    public async Task HandleAsync_TopicItDoesNotHandle_ThrowsInvalidEvent_AndReleasesNothing(string topic)
    {
        var act = () => CreateHandler().HandleAsync(topic, CancelledPayload(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<InvalidIntegrationEventException>();
        VerifyNothingReleased();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{")]
    [InlineData("null")]
    public async Task HandleAsync_UnreadablePayload_ThrowsInvalidEvent_AndReleasesNothing(string payload)
    {
        var act = () => CreateHandler().HandleAsync(EventTopics.OrderCancelled, payload, default);

        await act.Should().ThrowAsync<InvalidIntegrationEventException>();
        VerifyNothingReleased();
    }

    [Fact]
    public async Task HandleAsync_EmptyOrderId_ThrowsInvalidEvent_AndReleasesNothing()
    {
        var payload = JsonSerializer.Serialize(new { OrderId = Guid.Empty });

        var act = () => CreateHandler().HandleAsync(EventTopics.OrderCancelled, payload, default);

        await act.Should().ThrowAsync<InvalidIntegrationEventException>();
        VerifyNothingReleased();
    }

    [Fact]
    public async Task HandleAsync_PayloadWithoutOrderId_ThrowsInvalidEvent()
    {
        var act = () => CreateHandler().HandleAsync(EventTopics.OrderCancelled, "{}", default);

        await act.Should().ThrowAsync<InvalidIntegrationEventException>();
        VerifyNothingReleased();
    }

    [Fact]
    public async Task HandleAsync_ReleaseFails_PropagatesTheOriginalErrorSoTheConsumerCanRetry()
    {
        // Lỗi tạm thời (vd DB down) KHÔNG được biến thành InvalidIntegrationEventException, nếu không consumer sẽ bỏ qua message
        _sender
            .Setup(s => s.Send(It.IsAny<ReleaseStockCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var act = () => CreateHandler().HandleAsync(
            EventTopics.OrderCancelled, CancelledPayload(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("db down");
    }

    [Fact]
    public void Topics_AreExactlyTheEventThatInvalidatesAReservation()
    {
        StockReleaseEventHandler.Topics.Should().BeEquivalentTo(new[] { "smartshop.order.cancelled" });
    }
}
