using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data.Configurations;

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired(false);

        // Map private backing field _messages
        builder.HasMany(s => s.Messages)
            .WithOne()
            .HasForeignKey(m => m.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Messages)
            .HasField("_messages")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField);
    }
}
