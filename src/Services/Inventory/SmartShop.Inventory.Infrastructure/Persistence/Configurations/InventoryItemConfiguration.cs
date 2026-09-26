using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Inventory.Domain.Entities;

namespace SmartShop.Inventory.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.QuantityAvailable).IsRequired();
        builder.Property(x => x.QuantityReserved).IsRequired();
        builder.Property(x => x.LowStockThreshold).IsRequired();

        // Optimistic concurrency: cột hệ thống xmin của PostgreSQL (shadow property, entity không có field này).
        // Tương đương RowVersion của Core; UPDATE lệch xmin → DbUpdateConcurrencyException.
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .IsRowVersion();

        // Unique index có filter: NULL trong unique index PostgreSQL bị coi là "khác nhau",
        // nên tách riêng sản phẩm không size (size_id IS NULL) và có size.
        builder.HasIndex(x => new { x.StoreId, x.ProductId })
            .IsUnique()
            .HasFilter("size_id IS NULL");

        builder.HasIndex(x => new { x.StoreId, x.ProductId, x.SizeId })
            .IsUnique()
            .HasFilter("size_id IS NOT NULL");

        // Chốt bất biến ở tầng DB: 0 <= reserved <= available
        builder.ToTable(t => t.HasCheckConstraint(
            "ck_inventory_items_quantities",
            "quantity_available >= 0 AND quantity_reserved >= 0 AND quantity_reserved <= quantity_available"));
    }
}
