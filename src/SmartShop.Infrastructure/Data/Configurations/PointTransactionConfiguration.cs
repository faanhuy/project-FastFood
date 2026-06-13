using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AccountId).IsRequired();
        builder.Property(e => e.Points).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.Type).HasConversion<int>().IsRequired();
        builder.Property(e => e.Note).HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.AccountId)
            .HasDatabaseName("IX_PointTransactions_AccountId");
        builder.HasIndex(e => new { e.AccountId, e.CreatedAt })
            .HasDatabaseName("IX_PointTransactions_Account_CreatedAt");

        // Many:1 PointTransaction → LoyaltyAccount
        builder.HasOne(e => e.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
