using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Inventory.Domain.Entities;

namespace SmartShop.Inventory.Infrastructure.Persistence.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).IsRequired();

        // enum ReservationStatus lưu dạng int: Reserved=0, Confirmed=1, Released=2
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(x => x.InventoryItem)
            .WithMany()
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Một đơn không được giữ cùng một item hai lần (tầng DB chặn trùng),
        // đồng thời phục vụ truy vấn theo OrderId (OrderId là cột đầu của index).
        builder.HasIndex(x => new { x.OrderId, x.InventoryItemId })
            .IsUnique();
    }
}
