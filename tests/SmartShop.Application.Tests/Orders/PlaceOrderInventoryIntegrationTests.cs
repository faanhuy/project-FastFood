using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Orders.Commands.PlaceOrder;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using Xunit;
using CartEntity = SmartShop.Domain.Entities.Cart;

namespace SmartShop.Application.Tests.Orders;

/// <summary>
/// PlaceOrder khóa Redis theo sản phẩm → giữ chỗ ở Inventory Service (gRPC) → lưu Order,
/// và nhả khóa / bù trừ reservation ở mọi đường thoát.
/// </summary>
public class PlaceOrderInventoryIntegrationTests
{
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IStoreRepository> _storeRepo = new();
    private readonly Mock<IStoreInventoryRepository> _storeInventoryRepo = new();
    private readonly Mock<IStoreSizeInventoryRepository> _storeSizeInventoryRepo = new();
    private readonly Mock<ICouponRepository> _couponRepo = new();
    private readonly Mock<ICouponUsageRepository> _couponUsageRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUserAddressRepository> _userAddressRepo = new();
    private readonly Mock<IPriceCampaignRepository> _priceCampaignRepo = new();
    private readonly Mock<IFlashSaleRepository> _flashSaleRepo = new();
    private readonly Mock<IOrderFlashSaleUsageRepository> _orderFlashSaleUsageRepo = new();
    private readonly Mock<ILoyaltyRepository> _loyaltyRepo = new();
    private readonly Mock<IOutboxRepository> _outboxRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IInventoryClient> _inventoryClient = new();
    private readonly Mock<IDistributedLock> _distributedLock = new();
    private readonly Guid _storeId = Guid.NewGuid();
    private readonly Guid _addressId = Guid.NewGuid();

    // Ghi lại những gì handler đã làm với khóa và Inventory
    private readonly List<string> _lockKeys = new();
    private readonly List<Mock<IAsyncDisposable>> _handles = new();
    private Guid? _reservedOrderId;
    private IReadOnlyList<InventoryStockLine>? _reservedLines;

    public PlaceOrderInventoryIntegrationTests()
    {
        _priceCampaignRepo
            .Setup(r => r.GetEffectivePriceItemsAsync(
                It.IsAny<Guid>(),
                It.IsAny<IEnumerable<(Guid, Guid?)>>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<(Guid, Guid?), (int, decimal)>());

        _flashSaleRepo
            .Setup(r => r.GetActiveByProductIdAsync(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlashSale?)null);

        // Mặc định: giành được mọi khóa, Inventory giữ chỗ thành công, lưu DB thành công
        _distributedLock
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns((string key, TimeSpan _, CancellationToken _) => Task.FromResult<IAsyncDisposable?>(TrackHandle(key)));

        _inventoryClient
            .Setup(c => c.CheckAndReserveStockAsync(
                It.IsAny<Guid>(), It.IsAny<IReadOnlyList<InventoryStockLine>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, IReadOnlyList<InventoryStockLine>, CancellationToken>((orderId, lines, _) =>
            {
                _reservedOrderId = orderId;
                _reservedLines = lines;
            })
            .ReturnsAsync(new ReserveStockOutcome(true, Array.Empty<InventoryStockLineResult>()));

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private IAsyncDisposable TrackHandle(string key)
    {
        _lockKeys.Add(key);
        var handle = new Mock<IAsyncDisposable>();
        _handles.Add(handle);
        return handle.Object;
    }

    private PlaceOrderCommandHandler CreateHandler() =>
        new(_cartRepo.Object, _orderRepo.Object, _productRepo.Object,
            _storeRepo.Object, _storeInventoryRepo.Object, _storeSizeInventoryRepo.Object,
            _couponRepo.Object, _couponUsageRepo.Object,
            _userRepo.Object, _userAddressRepo.Object,
            _priceCampaignRepo.Object,
            _flashSaleRepo.Object,
            _orderFlashSaleUsageRepo.Object,
            _loyaltyRepo.Object,
            _outboxRepo.Object,
            _uow.Object, _mediator.Object,
            _inventoryClient.Object, _distributedLock.Object, NullLogger<PlaceOrderCommandHandler>.Instance);

    private PlaceOrderCommand Command(Guid userId, string? couponCode = null) =>
        new(userId, _storeId, _addressId, null, couponCode);

    // ── Arrange helpers ──────────────────────────────────────────────────────

    private void SetupStoreAndAddress()
    {
        _storeRepo.Setup(r => r.GetByIdAsync(_storeId, default)).ReturnsAsync(Store.Create("Store", "0901234567"));
        _userAddressRepo
            .Setup(r => r.GetByIdAsync(_addressId, default))
            .ReturnsAsync(UserAddress.Create(Guid.NewGuid(), "Home", "Test User", "0901234567", "123 Main St", null, null));
    }

    private void SetupInventories(params (Guid ProductId, int Quantity)[] stock)
    {
        var inventories = stock.Select(s => StoreInventory.Create(_storeId, s.ProductId, s.Quantity)).ToArray();
        _storeInventoryRepo
            .Setup(r => r.GetByStoreAndProductsAsync(_storeId, It.IsAny<IEnumerable<Guid>>(), default))
            .ReturnsAsync(inventories);
    }

    private void SetupSizeInventories(Guid productId, params (Guid SizeId, int Quantity)[] stock)
    {
        var inventories = stock
            .Select(s => StoreSizeInventory.Create(_storeId, productId, s.SizeId, s.Quantity))
            .ToList();
        _storeSizeInventoryRepo
            .Setup(r => r.GetByStoreAndSizesAsync(_storeId, It.IsAny<IEnumerable<Guid>>(), default))
            .ReturnsAsync(inventories);
    }

    private static Product SizedProduct(string name)
    {
        var product = Product.Create(name, "Desc", 200m, Guid.NewGuid(), name.ToLowerInvariant());
        typeof(Product).GetProperty(nameof(Product.HasSizes))!.SetValue(product, true);
        return product;
    }

    /// <summary>Giỏ có 1 sản phẩm không size.</summary>
    private (Guid UserId, Guid ProductId) ArrangePlainProduct(int quantity = 2, int stock = 10, string name = "Laptop")
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = CartEntity.Create(userId);
        cart.AddItem(productId, name, null, quantity, 100m);

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(cart);
        _productRepo
            .Setup(r => r.GetByIdAsync(productId, default))
            .ReturnsAsync(Product.Create(name, "Desc", 100m, Guid.NewGuid(), name.ToLowerInvariant()));
        SetupStoreAndAddress();
        SetupInventories((productId, stock));

        return (userId, productId);
    }

    /// <summary>Giỏ có 1 sản phẩm size M.</summary>
    private (Guid UserId, Guid ProductId, Guid SizeId) ArrangeSizedProduct(int quantity = 2, int stock = 10)
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sizeId = Guid.NewGuid();
        var cart = CartEntity.Create(userId);
        cart.AddItem(productId, "T-Shirt", null, quantity, 200m, sizeId, "M");

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(cart);
        _productRepo.Setup(r => r.GetByIdAsync(productId, default)).ReturnsAsync(SizedProduct("T-Shirt"));
        SetupStoreAndAddress();
        SetupInventories((productId, stock));
        SetupSizeInventories(productId, (sizeId, stock));

        return (userId, productId, sizeId);
    }

    /// <summary>Giỏ có 1 combo (số lượng combo × số lượng mỗi combo = TotalQuantity của thành phần).</summary>
    private (Guid UserId, Guid ComponentProductId) ArrangeCombo(int comboQuantity, int qtyPerCombo, int stock)
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var comboId = Guid.NewGuid();
        var cart = CartEntity.Create(userId);

        var comboItem = CartItem.CreateCombo(cart.Id, comboId, "Combo", null, comboQuantity, 99m);
        comboItem.AddComponent(
            CartItemComponent.Create(comboItem.Id, productId, "Component", null, null, qtyPerCombo, comboQuantity, 30m));
        cart.AddComboItem(comboId, "Combo", null, comboQuantity, 99m, comboItem.Components);

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(cart);
        SetupStoreAndAddress();
        SetupInventories((productId, stock));

        return (userId, productId);
    }

    private void ReserveFails(params InventoryStockLineResult[] results) =>
        _inventoryClient
            .Setup(c => c.CheckAndReserveStockAsync(
                It.IsAny<Guid>(), It.IsAny<IReadOnlyList<InventoryStockLine>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReserveStockOutcome(false, results));

    private void AssertAllLocksReleasedOnce()
    {
        _handles.Should().NotBeEmpty();
        foreach (var handle in _handles)
            handle.Verify(h => h.DisposeAsync(), Times.Once());
    }

    private void AssertNoOrderSaved()
    {
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    // ── Đường thành công ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCart_ReservesPlainProductWithOrderIdAndStore()
    {
        var (userId, productId) = ArrangePlainProduct(quantity: 2);

        var result = await CreateHandler().Handle(Command(userId), default);

        _reservedOrderId.Should().Be(result.Id);
        _reservedLines.Should().BeEquivalentTo(new[] { new InventoryStockLine(productId, _storeId, null, 2) });
    }

    [Fact]
    public async Task Handle_SizedProduct_ReservationLineCarriesSizeId()
    {
        var (userId, productId, sizeId) = ArrangeSizedProduct(quantity: 3);

        await CreateHandler().Handle(Command(userId), default);

        _reservedLines.Should().BeEquivalentTo(new[] { new InventoryStockLine(productId, _storeId, sizeId, 3) });
    }

    [Fact]
    public async Task Handle_ComboItem_FlattensComponentsWithTotalQuantity()
    {
        // 2 combo × 3 sản phẩm mỗi combo = 6
        var (userId, componentProductId) = ArrangeCombo(comboQuantity: 2, qtyPerCombo: 3, stock: 20);

        await CreateHandler().Handle(Command(userId), default);

        _reservedLines.Should().BeEquivalentTo(new[] { new InventoryStockLine(componentProductId, _storeId, null, 6) });
    }

    [Fact]
    public async Task Handle_Success_ReleasesEveryLockExactlyOnce_AndNeverReleasesReservation()
    {
        var (userId, productId) = ArrangePlainProduct();

        await CreateHandler().Handle(Command(userId), default);

        _lockKeys.Should().ContainSingle().Which.Should().Be($"lock:inventory:{_storeId}:{productId}");
        AssertAllLocksReleasedOnce();
        _inventoryClient.Verify(c => c.ReleaseStockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    // ── Khóa ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_TwoProducts_AcquiresOneSortedLockPerProductWithFiveSecondTtl()
    {
        var userId = Guid.NewGuid();
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        var cart = CartEntity.Create(userId);
        cart.AddItem(productA, "A", null, 1, 100m);
        cart.AddItem(productB, "B", null, 1, 100m);

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(cart);
        _productRepo.Setup(r => r.GetByIdAsync(productA, default)).ReturnsAsync(Product.Create("A", "Desc", 100m, Guid.NewGuid(), "a"));
        _productRepo.Setup(r => r.GetByIdAsync(productB, default)).ReturnsAsync(Product.Create("B", "Desc", 100m, Guid.NewGuid(), "b"));
        SetupStoreAndAddress();
        SetupInventories((productA, 5), (productB, 5));

        await CreateHandler().Handle(Command(userId), default);

        _lockKeys.Should().HaveCount(2).And.OnlyHaveUniqueItems();
        _lockKeys.Should().Equal(_lockKeys.OrderBy(k => k, StringComparer.Ordinal));
        _distributedLock.Verify(
            l => l.TryAcquireAsync(It.IsAny<string>(), TimeSpan.FromSeconds(5), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_SameProductTwoSizes_AcquiresSingleLock()
    {
        // Key không chứa sizeId: 2 size cùng sản phẩm phải dùng chung 1 khóa, không tự khóa chính mình
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sizeS = Guid.NewGuid();
        var sizeM = Guid.NewGuid();
        var cart = CartEntity.Create(userId);
        cart.AddItem(productId, "T-Shirt", null, 1, 200m, sizeS, "S");
        cart.AddItem(productId, "T-Shirt", null, 1, 200m, sizeM, "M");

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(cart);
        _productRepo.Setup(r => r.GetByIdAsync(productId, default)).ReturnsAsync(SizedProduct("T-Shirt"));
        SetupStoreAndAddress();
        SetupInventories((productId, 10));
        SetupSizeInventories(productId, (sizeS, 5), (sizeM, 5));

        await CreateHandler().Handle(Command(userId), default);

        _lockKeys.Should().ContainSingle();
        _reservedLines.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_LockHeldByAnotherRequest_ThrowsConflict_WithoutTouchingInventoryOrDb()
    {
        var (userId, _) = ArrangePlainProduct();
        _distributedLock
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IAsyncDisposable?)null);

        var act = () => CreateHandler().Handle(Command(userId), default);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.MessageKey.Should().Be("error.order_inventory_locked");
        _inventoryClient.Verify(c => c.CheckAndReserveStockAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyList<InventoryStockLine>>(), It.IsAny<CancellationToken>()), Times.Never());
        AssertNoOrderSaved();
    }

    [Fact]
    public async Task Handle_SecondLockHeldByAnotherRequest_ReleasesTheFirstOne()
    {
        var userId = Guid.NewGuid();
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        var cart = CartEntity.Create(userId);
        cart.AddItem(productA, "A", null, 1, 100m);
        cart.AddItem(productB, "B", null, 1, 100m);

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(cart);
        _productRepo.Setup(r => r.GetByIdAsync(productA, default)).ReturnsAsync(Product.Create("A", "Desc", 100m, Guid.NewGuid(), "a"));
        _productRepo.Setup(r => r.GetByIdAsync(productB, default)).ReturnsAsync(Product.Create("B", "Desc", 100m, Guid.NewGuid(), "b"));
        SetupStoreAndAddress();
        SetupInventories((productA, 5), (productB, 5));

        var calls = 0;
        _distributedLock
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns((string key, TimeSpan _, CancellationToken _) =>
                Task.FromResult<IAsyncDisposable?>(++calls == 1 ? TrackHandle(key) : null));

        var act = () => CreateHandler().Handle(Command(userId), default);

        await act.Should().ThrowAsync<ConflictException>();
        _handles.Should().ContainSingle();
        AssertAllLocksReleasedOnce();
    }

    [Fact]
    public async Task Handle_LockBackendUnavailable_Propagates_AndNeverReserves()
    {
        var (userId, _) = ArrangePlainProduct();
        _distributedLock
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ServiceUnavailableException("redis down"));

        var act = () => CreateHandler().Handle(Command(userId), default);

        await act.Should().ThrowAsync<ServiceUnavailableException>();
        _inventoryClient.Verify(c => c.CheckAndReserveStockAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyList<InventoryStockLine>>(), It.IsAny<CancellationToken>()), Times.Never());
        AssertNoOrderSaved();
    }

    [Fact]
    public async Task Handle_EmptyCart_NeverAttemptsToLock()
    {
        var userId = Guid.NewGuid();
        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(CartEntity.Create(userId));

        var act = () => CreateHandler().Handle(Command(userId), default);

        await act.Should().ThrowAsync<ConflictException>();
        _distributedLock.Verify(
            l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    // ── Inventory từ chối giữ chỗ ────────────────────────────────────────────

    [Fact]
    public async Task Handle_InventoryShortOnPlainProduct_ThrowsInsufficientWithNameAndQty()
    {
        var (userId, productId) = ArrangePlainProduct(quantity: 5, name: "Laptop");
        ReserveFails(new InventoryStockLineResult(productId, null, false, 1));

        var act = () => CreateHandler().Handle(Command(userId), default);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.MessageKey.Should().Be("error.order_inventory_insufficient");
        ex.Which.Params.Should().Contain("name", "Laptop").And.Contain("qty", "1");
        AssertNoOrderSaved();
        AssertAllLocksReleasedOnce();
        // Reserve all-or-nothing đã thất bại → chưa giữ chỗ gì, không được gọi Release
        _inventoryClient.Verify(c => c.ReleaseStockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_InventoryShortOnSizedProduct_ThrowsInsufficientSizeWithSizeLabel()
    {
        var (userId, productId, sizeId) = ArrangeSizedProduct(quantity: 4);
        ReserveFails(new InventoryStockLineResult(productId, sizeId, false, 2));

        var act = () => CreateHandler().Handle(Command(userId), default);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.MessageKey.Should().Be("error.order_inventory_insufficient_size");
        ex.Which.Params.Should().Contain("name", "T-Shirt").And.Contain("size", "M").And.Contain("qty", "2");
    }

    [Fact]
    public async Task Handle_InventoryShortOnComboComponent_ThrowsComboInsufficient()
    {
        var (userId, componentProductId) = ArrangeCombo(comboQuantity: 1, qtyPerCombo: 2, stock: 10);
        ReserveFails(new InventoryStockLineResult(componentProductId, null, false, 1));

        var act = () => CreateHandler().Handle(Command(userId), default);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.MessageKey.Should().Be("error.order_combo_insufficient");
        ex.Which.Params.Should().Contain("name", "Component").And.Contain("qty", "1");
    }

    [Fact]
    public async Task Handle_InventoryFailsWithoutFailedLine_FallsBackToOutOfStockRace()
    {
        var (userId, _) = ArrangePlainProduct();
        ReserveFails(); // Success = false nhưng không dòng nào báo Reserved = false

        var act = () => CreateHandler().Handle(Command(userId), default);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.MessageKey.Should().Be("error.order_out_of_stock_race");
    }

    [Fact]
    public async Task Handle_InventoryUnavailable_Propagates_ReleasesLocks_AndNeverSaves()
    {
        var (userId, _) = ArrangePlainProduct();
        _inventoryClient
            .Setup(c => c.CheckAndReserveStockAsync(
                It.IsAny<Guid>(), It.IsAny<IReadOnlyList<InventoryStockLine>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ServiceUnavailableException("inventory down"));

        var act = () => CreateHandler().Handle(Command(userId), default);

        await act.Should().ThrowAsync<ServiceUnavailableException>();
        AssertNoOrderSaved();
        AssertAllLocksReleasedOnce();
    }

    [Fact]
    public async Task Handle_CouponNotFound_FailsBeforeReserving_SoNoReservationLeaks()
    {
        // Giữ chỗ đặt sau mọi validate: đơn bị từ chối vì coupon không được để lại reservation ở Inventory
        var (userId, _) = ArrangePlainProduct();
        _couponRepo.Setup(r => r.GetByCodeAsync("NOPE", default)).ReturnsAsync((Coupon?)null);

        var act = () => CreateHandler().Handle(Command(userId, "NOPE"), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _inventoryClient.Verify(c => c.CheckAndReserveStockAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyList<InventoryStockLine>>(), It.IsAny<CancellationToken>()), Times.Never());
        AssertAllLocksReleasedOnce();
    }

    // ── Lưu Order thất bại sau khi đã giữ chỗ → bù trừ ───────────────────────

    [Fact]
    public async Task Handle_SaveFailsAfterReserve_ReleasesReservationForThatOrder_AndReleasesLocks()
    {
        var (userId, _) = ArrangePlainProduct();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var act = () => CreateHandler().Handle(Command(userId), default);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("db down");
        _reservedOrderId.Should().NotBeNull();
        _inventoryClient.Verify(c => c.ReleaseStockAsync(_reservedOrderId!.Value, It.IsAny<CancellationToken>()), Times.Once());
        AssertAllLocksReleasedOnce();
    }

    [Fact]
    public async Task Handle_StockRaceOnSaveAfterRetry_ThrowsOutOfStockRace_AndReleasesReservation()
    {
        var (userId, _) = ArrangePlainProduct();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyException("row version changed"));

        var act = () => CreateHandler().Handle(Command(userId), default);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.MessageKey.Should().Be("error.order_out_of_stock_race");
        _inventoryClient.Verify(c => c.ReleaseStockAsync(_reservedOrderId!.Value, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Handle_ReleaseFailsDuringCompensation_StillThrowsTheOriginalError()
    {
        var (userId, _) = ArrangePlainProduct();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));
        _inventoryClient
            .Setup(c => c.ReleaseStockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ServiceUnavailableException("inventory down"));

        var act = () => CreateHandler().Handle(Command(userId), default);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("db down");
        AssertAllLocksReleasedOnce();
    }
}
