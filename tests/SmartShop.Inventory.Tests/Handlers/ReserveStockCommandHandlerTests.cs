using FluentAssertions;
using Moq;
using SmartShop.Inventory.Application.Common.Interfaces;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReserveStock;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Enums;
using SmartShop.Inventory.Domain.Interfaces;
using SmartShop.Inventory.Tests.TestData;

namespace SmartShop.Inventory.Tests.Handlers;

public class ReserveStockCommandHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _items = new();
    private readonly Mock<IStockReservationRepository> _reservations = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly List<StockReservation> _added = [];

    public ReserveStockCommandHandlerTests()
    {
        _reservations
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockReservation>());
        _reservations
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<StockReservation>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<StockReservation>, CancellationToken>((rs, _) => _added.AddRange(rs))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private ReserveStockCommandHandler Sut => new(_items.Object, _reservations.Object, _unitOfWork.Object);

    private void SetupItems(params InventoryItem[] items) => SetupItemsFactory(() => items.ToList());

    // Factory: mỗi lần tải trả về dữ liệu mới — mô phỏng ChangeTracker.Clear() + tải lại sau xung đột xmin
    private void SetupItemsFactory(Func<List<InventoryItem>> factory) =>
        _items
            .Setup(r => r.GetByKeysAsync(
                It.IsAny<IReadOnlyCollection<(Guid StoreId, Guid ProductId, Guid? SizeId)>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(factory);

    private void VerifyItemsLoaded(Times times) =>
        _items.Verify(r => r.GetByKeysAsync(
            It.IsAny<IReadOnlyCollection<(Guid StoreId, Guid ProductId, Guid? SizeId)>>(),
            It.IsAny<CancellationToken>()), times);

    private static ReserveStockLine Line(InventoryItem item, int quantity) =>
        new(item.ProductId, item.StoreId, item.SizeId, quantity);

    // ── Thành công ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AllLinesAvailable_ReservesEveryItemAndSavesOnce()
    {
        var orderId = Guid.NewGuid();
        var a = StockTestData.Item(available: 10);
        var b = StockTestData.Item(available: 5);
        SetupItems(a, b);

        var result = await Sut.Handle(
            new ReserveStockCommand(orderId, [Line(a, 3), Line(b, 5)]), CancellationToken.None);

        result.Success.Should().BeTrue();
        a.QuantityReserved.Should().Be(3);
        b.QuantityReserved.Should().Be(5);
        _added.Should().HaveCount(2);
        _added.Should().OnlyContain(r => r.OrderId == orderId && r.Status == ReservationStatus.Reserved);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

        // AvailableQuantity trả về là số còn lại SAU khi giữ chỗ
        result.Results.Single(r => r.ProductId == a.ProductId).AvailableQuantity.Should().Be(7);
        result.Results.Single(r => r.ProductId == b.ProductId).AvailableQuantity.Should().Be(0);
    }

    // ── All-or-nothing ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_OneLineInsufficient_ReturnsFailureAndChangesNothing()
    {
        var enough = StockTestData.Item(available: 10);
        var tooFew = StockTestData.Item(available: 2);
        SetupItems(enough, tooFew);

        var result = await Sut.Handle(
            new ReserveStockCommand(Guid.NewGuid(), [Line(enough, 3), Line(tooFew, 5)]), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Results.Single(r => r.ProductId == enough.ProductId).Should().Match<ReserveStockLineResult>(
            r => r.Reserved && r.AvailableQuantity == 10);
        result.Results.Single(r => r.ProductId == tooFew.ProductId).Should().Match<ReserveStockLineResult>(
            r => !r.Reserved && r.AvailableQuantity == 2);

        // Dòng đủ hàng cũng KHÔNG được giữ chỗ
        enough.QuantityReserved.Should().Be(0);
        tooFew.QuantityReserved.Should().Be(0);
        _reservations.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<StockReservation>>(), It.IsAny<CancellationToken>()), Times.Never());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_ItemDoesNotExist_MarksLineNotReservedWithZeroAvailable()
    {
        SetupItems();   // không có item nào trong kho
        var line = new ReserveStockLine(Guid.NewGuid(), StockTestData.StoreId, null, 1);

        var result = await Sut.Handle(
            new ReserveStockCommand(Guid.NewGuid(), [line]), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Results.Should().ContainSingle()
            .Which.Should().Match<ReserveStockLineResult>(r => !r.Reserved && r.AvailableQuantity == 0);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    // ── Gộp dòng trùng khoá ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DuplicateKeys_AreMergedIntoOneReservation()
    {
        var item = StockTestData.Item(available: 10);
        SetupItems(item);

        var result = await Sut.Handle(
            new ReserveStockCommand(Guid.NewGuid(), [Line(item, 3), Line(item, 4)]), CancellationToken.None);

        result.Success.Should().BeTrue();
        item.QuantityReserved.Should().Be(7);
        _added.Should().ContainSingle().Which.Quantity.Should().Be(7);
    }

    [Fact]
    public async Task Handle_DuplicateKeys_AreCheckedAgainstTheSummedQuantity()
    {
        var item = StockTestData.Item(available: 6);   // mỗi dòng (3, 4) đều <= 6 nhưng tổng 7 > 6
        SetupItems(item);

        var result = await Sut.Handle(
            new ReserveStockCommand(Guid.NewGuid(), [Line(item, 3), Line(item, 4)]), CancellationToken.None);

        result.Success.Should().BeFalse();
        item.QuantityReserved.Should().Be(0);
    }

    [Fact]
    public async Task Handle_SameProductDifferentSize_AreSeparateLines()
    {
        var productId = Guid.NewGuid();
        var small = StockTestData.Item(available: 5, productId: productId, sizeId: Guid.NewGuid());
        var medium = StockTestData.Item(available: 5, productId: productId, sizeId: Guid.NewGuid());
        SetupItems(small, medium);

        var result = await Sut.Handle(
            new ReserveStockCommand(Guid.NewGuid(), [Line(small, 2), Line(medium, 3)]), CancellationToken.None);

        result.Success.Should().BeTrue();
        small.QuantityReserved.Should().Be(2);
        medium.QuantityReserved.Should().Be(3);
        _added.Should().HaveCount(2);
    }

    // ── Idempotency theo order_id ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_OrderAlreadyReserved_ReplaysWithoutReservingAgain()
    {
        var orderId = Guid.NewGuid();
        var item = StockTestData.Item(available: 10);
        item.Reserve(3);
        var existing = StockTestData.Reservation(item, orderId, 3);
        _reservations
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockReservation> { existing });

        var result = await Sut.Handle(
            new ReserveStockCommand(orderId, [Line(item, 3)]), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Results.Should().ContainSingle().Which.AvailableQuantity.Should().Be(7);
        item.QuantityReserved.Should().Be(3);   // không nhân đôi
        VerifyItemsLoaded(Times.Never());
        _reservations.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<StockReservation>>(), It.IsAny<CancellationToken>()), Times.Never());
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_OrderAlreadyReleased_ThrowsConflict()
    {
        var orderId = Guid.NewGuid();
        var item = StockTestData.Item(available: 10);
        var released = StockTestData.Reservation(item, orderId, 3, ReservationStatus.Released);
        _reservations
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockReservation> { released });

        var act = () => Sut.Handle(new ReserveStockCommand(orderId, [Line(item, 3)]), CancellationToken.None);

        (await act.Should().ThrowAsync<ConflictException>())
            .Which.MessageKey.Should().Be("error.inventory_order_already_released");
    }

    // ── Xung đột xmin (đường retry) ───────────────────────────────────────────

    [Fact]
    public async Task Handle_ConflictOnFirstSave_ReloadsItemsAndSucceedsOnRetry()
    {
        var productId = Guid.NewGuid();
        SetupItemsFactory(() => [StockTestData.Item(available: 10, productId: productId)]);
        _unitOfWork
            .SetupSequence(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyException("xmin changed"))
            .ReturnsAsync(1);
        var line = new ReserveStockLine(productId, StockTestData.StoreId, null, 4);

        var result = await Sut.Handle(
            new ReserveStockCommand(Guid.NewGuid(), [line]), CancellationToken.None);

        result.Success.Should().BeTrue();
        VerifyItemsLoaded(Times.Exactly(2));   // tải lại BÊN TRONG lượt retry, không dùng dữ liệu cũ
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ConflictOnEveryAttempt_RethrowsAfterTwoAttempts()
    {
        var productId = Guid.NewGuid();
        SetupItemsFactory(() => [StockTestData.Item(available: 10, productId: productId)]);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyException("xmin changed"));
        var line = new ReserveStockLine(productId, StockTestData.StoreId, null, 1);

        var act = () => Sut.Handle(new ReserveStockCommand(Guid.NewGuid(), [line]), CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyException>();
        VerifyItemsLoaded(Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ConflictThenStockRunsOut_ReturnsFailureInsteadOfOverselling()
    {
        // Lần 1: item còn hàng nhưng SaveChanges đụng xung đột. Lần 2: request khác đã lấy hết → tải lại thấy hết hàng.
        var productId = Guid.NewGuid();
        var loads = 0;
        SetupItemsFactory(() =>
        {
            loads++;
            return [StockTestData.Item(available: loads == 1 ? 1 : 0, productId: productId)];
        });
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyException("xmin changed"));
        var line = new ReserveStockLine(productId, StockTestData.StoreId, null, 1);

        var result = await Sut.Handle(
            new ReserveStockCommand(Guid.NewGuid(), [line]), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Results.Should().ContainSingle().Which.Reserved.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());   // lần 2 không lưu gì
    }
}
