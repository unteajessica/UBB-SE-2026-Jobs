using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(message => message.MessageId);

        builder.Property(message => message.Content).HasMaxLength(4000).IsRequired();
        builder.Property(message => message.OriginalFileName).HasMaxLength(500);

        // View-only fields — not persisted in the database.
        builder.Ignore(message => message.ShowReadReceipt);
        builder.Ignore(message => message.SenderInitials);

        // Messages are owned by a chat; cascade so that deleting a chat removes its messages.
        builder.HasOne<Chat>()
            .WithMany()
            .HasForeignKey(message => message.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on ChatId supports the common GetForChatAsync query.
        builder.HasIndex(message => message.ChatId);
    }
}
