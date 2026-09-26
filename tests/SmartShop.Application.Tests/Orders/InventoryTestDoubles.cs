using Moq;
using SmartShop.Application.Common.Interfaces;

namespace SmartShop.Application.Tests.Orders;

/// <summary>
/// Mặc định "êm" cho 2 phụ thuộc hạ tầng của <c>PlaceOrderCommandHandler</c> (Inventory gRPC + khóa phân tán),
/// để các test cũ không liên quan tới Inventory vẫn chạy đường thành công mà không phải tự setup.
/// </summary>
internal static class InventoryTestDoubles
{
    /// <summary>Inventory luôn giữ chỗ thành công.</summary>
    public static Mock<IInventoryClient> ReservingClient()
    {
        var client = new Mock<IInventoryClient>();
        client
            .Setup(c => c.CheckAndReserveStockAsync(
                It.IsAny<Guid>(), It.IsAny<IReadOnlyList<InventoryStockLine>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReserveStockOutcome(true, Array.Empty<InventoryStockLineResult>()));
        return client;
    }

    /// <summary>Luôn giành được khóa.</summary>
    public static Mock<IDistributedLock> GrantingLock()
    {
        var locker = new Mock<IDistributedLock>();
        locker
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new Mock<IAsyncDisposable>().Object);
        return locker;
    }
}
