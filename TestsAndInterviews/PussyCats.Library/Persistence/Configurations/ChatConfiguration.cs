using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("Chats");
        builder.HasKey(chat => chat.ChatId);

        // View-only fields — not persisted in the database.
        builder.Ignore(chat => chat.LastMessageSnippet);
        builder.Ignore(chat => chat.LastMessageTime);
        builder.Ignore(chat => chat.LastMessage);
        builder.Ignore(chat => chat.UnreadCount);

        // UserId is the primary participant (always a user/candidate).
        builder.HasOne(chat => chat.User)
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Restrict);

        // SecondUser is the other user in a user-to-user chat (nullable).
        builder.HasOne(chat => chat.SecondUser)
            .WithMany()
            .HasForeignKey("SecondUserId")
            .OnDelete(DeleteBehavior.Restrict);

        // CompanyId is set for user-to-company chats (nullable).
        builder.HasOne(chat => chat.Company)
            .WithMany()
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Restrict);

        // JobId links the chat to a specific job posting (nullable).
        builder.HasOne(chat=>chat.Job)
            .WithMany()
            .HasForeignKey("JobId")
            .OnDelete(DeleteBehavior.Restrict);

        // BlockedByUser is the user who blocked the chat (nullable).
        builder.HasOne(chat => chat.BlockedByUser)
            .WithMany()
            .HasForeignKey("BlockedByUserId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
