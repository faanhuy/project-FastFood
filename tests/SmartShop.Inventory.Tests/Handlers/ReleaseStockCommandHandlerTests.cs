using FluentAssertions;
using Moq;
using SmartShop.Inventory.Application.Common.Interfaces;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Enums;
using SmartShop.Inventory.Domain.Interfaces;
using SmartShop.Inventory.Tests.TestData;

namespace SmartShop.Inventory.Tests.Handlers;

public class ReleaseStockCommandHandlerTests
{
    private readonly Mock<IStockReservationRepository> _reservations = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public ReleaseStockCommandHandlerTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private ReleaseStockCommandHandler Sut => new(_reservations.Object, _unitOfWork.Object);

    private void SetupOrder(Guid orderId, params StockReservation[] reservations) =>
        _reservations
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations.ToList());

    private void VerifySaved(Times times) =>
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), times);

    [Fact]
    public async Task Handle_ReservedReservation_ReleasesTheItemAndMarksReleased()
    {
        var orderId = Guid.NewGuid();
        var item = StockTestData.Item(available: 10);
        item.Reserve(4);
        var reservation = StockTestData.Reservation(item, orderId, 4);
        SetupOrder(orderId, reservation);

        await Sut.Handle(new ReleaseStockCommand(orderId), CancellationToken.None);

        item.QuantityReserved.Should().Be(0);
        item.Sellable.Should().Be(10);
        reservation.Status.Should().Be(ReservationStatus.Released);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Handle_ConfirmedReservation_IsLeftUntouched()
    {
        // Confirmed = hàng đã xuất kho, không được trả chỗ nữa
        var orderId = Guid.NewGuid();
        var item = StockTestData.Item(available: 10);
        item.Reserve(4);
        var confirmed = StockTestData.Reservation(item, orderId, 4, ReservationStatus.Confirmed);
        SetupOrder(orderId, confirmed);

        await Sut.Handle(new ReleaseStockCommand(orderId), CancellationToken.None);

        item.QuantityReserved.Should().Be(4);
        confirmed.Status.Should().Be(ReservationStatus.Confirmed);
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Handle_MixedStatuses_OnlyReservedOnesAreReleased()
    {
        var orderId = Guid.NewGuid();
        var itemA = StockTestData.Item(available: 10);
        itemA.Reserve(2);
        var itemB = StockTestData.Item(available: 10);
        itemB.Reserve(3);
        var toRelease = StockTestData.Reservation(itemA, orderId, 2);
        var alreadyConfirmed = StockTestData.Reservation(itemB, orderId, 3, ReservationStatus.Confirmed);
        SetupOrder(orderId, toRelease, alreadyConfirmed);

        await Sut.Handle(new ReleaseStockCommand(orderId), CancellationToken.None);

        itemA.QuantityReserved.Should().Be(0);
        toRelease.Status.Should().Be(ReservationStatus.Released);
        itemB.QuantityReserved.Should().Be(3);
        alreadyConfirmed.Status.Should().Be(ReservationStatus.Confirmed);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Handle_OrderHasNoReservation_SucceedsWithoutSaving()
    {
        var orderId = Guid.NewGuid();
        SetupOrder(orderId);

        var act = () => Sut.Handle(new ReleaseStockCommand(orderId), CancellationToken.None);

        await act.Should().NotThrowAsync();
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Handle_CalledTwiceForTheSameOrder_SecondCallIsANoOp()
    {
        // Kafka at-least-once (Sprint 38) có thể gửi lại cùng 1 event
        var orderId = Guid.NewGuid();
        var item = StockTestData.Item(available: 10);
        item.Reserve(4);
        SetupOrder(orderId, StockTestData.Reservation(item, orderId, 4));

        await Sut.Handle(new ReleaseStockCommand(orderId), CancellationToken.None);
        await Sut.Handle(new ReleaseStockCommand(orderId), CancellationToken.None);

        item.QuantityReserved.Should().Be(0);   // không âm, không ném lỗi
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Handle_ConflictOnFirstSave_ReloadsAndReleasesOnRetry()
    {
        var orderId = Guid.NewGuid();
        InventoryItem? latestItem = null;
        _reservations
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                // Mỗi lần tải là dữ liệu mới — mô phỏng ChangeTracker.Clear() sau xung đột
                latestItem = StockTestData.Item(available: 10);
                latestItem.Reserve(4);
                return new List<StockReservation> { StockTestData.Reservation(latestItem, orderId, 4) };
            });
        _unitOfWork
            .SetupSequence(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyException("xmin changed"))
            .ReturnsAsync(1);

        await Sut.Handle(new ReleaseStockCommand(orderId), CancellationToken.None);

        _reservations.Verify(
            r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        latestItem!.QuantityReserved.Should().Be(0);
        VerifySaved(Times.Exactly(2));
    }
}
