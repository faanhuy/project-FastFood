using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.TotalPoints).HasPrecision(18, 2);
        builder.Property(e => e.LifetimePoints).HasPrecision(18, 2);
        builder.Property(e => e.Tier).IsRequired();

        // Unique index on UserId
        builder.HasIndex(e => e.UserId).IsUnique();

        // Many:1 LoyaltyAccount → User
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tell EF to use the private backing field so change tracking detects EarnPoints/ReversePoints mutations
        builder.Navigation(e => e.Transactions)
            .HasField("_transactions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
