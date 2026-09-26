using FluentAssertions;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Enums;

namespace SmartShop.Inventory.Tests.Entities;

public class StockReservationTests
{
    private const string InvalidStatusKey = "error.inventory_reservation_invalid_status";

    private static StockReservation NewReservation() =>
        StockReservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_StartsReservedWithUtcTimestamp()
    {
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var reservation = StockReservation.Create(orderId, itemId, 3);

        reservation.OrderId.Should().Be(orderId);
        reservation.InventoryItemId.Should().Be(itemId);
        reservation.Quantity.Should().Be(3);
        reservation.Status.Should().Be(ReservationStatus.Reserved);
        reservation.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);   // Npgsql timestamptz bắt buộc UTC
        reservation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_EmptyOrderId_Throws()
    {
        var act = () => StockReservation.Create(Guid.Empty, Guid.NewGuid(), 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyInventoryItemId_Throws()
    {
        var act = () => StockReservation.Create(Guid.NewGuid(), Guid.Empty, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_NonPositiveQuantity_Throws(int quantity)
    {
        var act = () => StockReservation.Create(Guid.NewGuid(), Guid.NewGuid(), quantity);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Đường chuyển trạng thái hợp lệ: Reserved → Confirmed | Released ───────

    [Fact]
    public void MarkConfirmed_FromReserved_SetsConfirmed()
    {
        var reservation = NewReservation();

        reservation.MarkConfirmed();

        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void MarkReleased_FromReserved_SetsReleased()
    {
        var reservation = NewReservation();

        reservation.MarkReleased();

        reservation.Status.Should().Be(ReservationStatus.Released);
    }

    // ── Không có đường quay lại ───────────────────────────────────────────────

    [Fact]
    public void MarkConfirmed_WhenReleased_ThrowsConflictAndKeepsStatus()
    {
        var reservation = NewReservation();
        reservation.MarkReleased();

        var act = () => reservation.MarkConfirmed();

        var ex = act.Should().Throw<ConflictException>().Which;
        ex.MessageKey.Should().Be(InvalidStatusKey);
        ex.Params.Should().Contain("status", nameof(ReservationStatus.Released));
        reservation.Status.Should().Be(ReservationStatus.Released);
    }

    [Fact]
    public void MarkReleased_WhenConfirmed_ThrowsConflictAndKeepsStatus()
    {
        var reservation = NewReservation();
        reservation.MarkConfirmed();

        var act = () => reservation.MarkReleased();

        act.Should().Throw<ConflictException>().Which.MessageKey.Should().Be(InvalidStatusKey);
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void MarkConfirmed_WhenAlreadyConfirmed_ThrowsConflict()
    {
        var reservation = NewReservation();
        reservation.MarkConfirmed();

        var act = () => reservation.MarkConfirmed();

        act.Should().Throw<ConflictException>().Which.MessageKey.Should().Be(InvalidStatusKey);
    }

    [Fact]
    public void MarkReleased_WhenAlreadyReleased_ThrowsConflict()
    {
        var reservation = NewReservation();
        reservation.MarkReleased();

        var act = () => reservation.MarkReleased();

        act.Should().Throw<ConflictException>().Which.MessageKey.Should().Be(InvalidStatusKey);
    }
}
