using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ChatSessionId)
            .IsRequired();

        builder.Property(e => e.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Content)
            .IsRequired();

        // Performance index for querying by ChatSessionId
        builder.HasIndex(e => e.ChatSessionId);
    }
}
