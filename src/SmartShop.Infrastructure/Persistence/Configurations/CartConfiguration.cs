using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(e => e.Id);

        // TotalAmount is a C# computed property (sum of items) — not a DB column
        builder.Ignore(e => e.TotalAmount);

        // 1:1 Cart → User (cascade: delete cart when user is deleted)
        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<Cart>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:many Cart → CartItems
        // Explicitly use backing field _items for reads and writes
        builder.Navigation(e => e.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
