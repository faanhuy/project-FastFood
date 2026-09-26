using FluentAssertions;
using Moq;
using SmartShop.Inventory.Application.Common.Interfaces;
using SmartShop.Inventory.Application.Features.Stock.Commands.ConfirmStock;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Enums;
using SmartShop.Inventory.Domain.Interfaces;
using SmartShop.Inventory.Tests.TestData;
using Xunit;

namespace SmartShop.Inventory.Tests.Handlers;

public class ConfirmStockCommandHandlerTests
{
    private readonly Mock<IStockReservationRepository> _reservations = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public ConfirmStockCommandHandlerTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private ConfirmStockCommandHandler Sut => new(_reservations.Object, _unitOfWork.Object);

    private void SetupOrder(Guid orderId, params StockReservation[] reservations) =>
        _reservations
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations.ToList());

    private void VerifySaved(Times times) =>
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), times);

    [Fact]
    public async Task Handle_OrderHasNoReservation_ThrowsNotFound()
    {
        var orderId = Guid.NewGuid();
        SetupOrder(orderId);

        var act = () => Sut.Handle(new ConfirmStockCommand(orderId), CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<NotFoundException>()).Which;
        ex.EntityName.Should().Be(nameof(StockReservation));
        ex.Key.Should().Be(orderId);
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Handle_ReservedReservation_ConfirmsAndReducesPhysicalStock()
    {
        var orderId = Guid.NewGuid();
        var item = StockTestData.Item(available: 10);
        item.Reserve(3);
        var reservation = StockTestData.Reservation(item, orderId, 3);
        SetupOrder(orderId, reservation);

        await Sut.Handle(new ConfirmStockCommand(orderId), CancellationToken.None);

        item.QuantityReserved.Should().Be(0);
        item.QuantityAvailable.Should().Be(7);
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Handle_SeveralItemsInOneOrder_ConfirmsAllInASingleSave()
    {
        var orderId = Guid.NewGuid();
        var itemA = StockTestData.Item(available: 10);
        itemA.Reserve(2);
        var itemB = StockTestData.Item(available: 5);
        itemB.Reserve(5);
        SetupOrder(orderId,
            StockTestData.Reservation(itemA, orderId, 2),
            StockTestData.Reservation(itemB, orderId, 5));

        await Sut.Handle(new ConfirmStockCommand(orderId), CancellationToken.None);

        itemA.QuantityAvailable.Should().Be(8);
        itemB.QuantityAvailable.Should().Be(0);
        itemA.QuantityReserved.Should().Be(0);
        itemB.QuantityReserved.Should().Be(0);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Handle_AlreadyConfirmed_SucceedsWithoutChangingStock()
    {
        // Idempotent: gọi lại Confirm cho đơn đã xác nhận không được trừ kho lần nữa
        var orderId = Guid.NewGuid();
        var item = StockTestData.Item(available: 7);
        var confirmed = StockTestData.Reservation(item, orderId, 3, ReservationStatus.Confirmed);
        SetupOrder(orderId, confirmed);

        await Sut.Handle(new ConfirmStockCommand(orderId), CancellationToken.None);

        item.QuantityAvailable.Should().Be(7);
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Handle_OnlyReleasedReservations_ThrowsConflict()
    {
        var orderId = Guid.NewGuid();
        var item = StockTestData.Item(available: 10);
        SetupOrder(orderId, StockTestData.Reservation(item, orderId, 3, ReservationStatus.Released));

        var act = () => Sut.Handle(new ConfirmStockCommand(orderId), CancellationToken.None);

        (await act.Should().ThrowAsync<ConflictException>())
            .Which.MessageKey.Should().Be("error.inventory_order_already_released");
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Handle_ConflictOnFirstSave_ReloadsAndConfirmsOnRetry()
    {
        var orderId = Guid.NewGuid();
        InventoryItem? latestItem = null;
        _reservations
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                latestItem = StockTestData.Item(available: 10);
                latestItem.Reserve(4);
                return new List<StockReservation> { StockTestData.Reservation(latestItem, orderId, 4) };
            });
        _unitOfWork
            .SetupSequence(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyException("xmin changed"))
            .ReturnsAsync(1);

        await Sut.Handle(new ConfirmStockCommand(orderId), CancellationToken.None);

        _reservations.Verify(
            r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        latestItem!.QuantityAvailable.Should().Be(6);
        latestItem.QuantityReserved.Should().Be(0);
        VerifySaved(Times.Exactly(2));
    }
}
