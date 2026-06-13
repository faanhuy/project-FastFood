using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data.Configurations;

public class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.OrderId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.Reason)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.EvidenceImageUrl)
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .IsRequired();

        builder.Property(r => r.AdminNote)
            .HasMaxLength(500);

        builder.Property(r => r.RefundAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.RefundedAt)
            .IsRequired(false);

        builder.Property(r => r.VnpayRefundTxnRef)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(r => r.RefundNote)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasIndex(r => r.OrderId)
            .HasDatabaseName("IX_ReturnRequests_OrderId");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_ReturnRequests_UserId");

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
