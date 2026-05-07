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
        builder.Ignore(chat => chat.OtherPartyName);

        // UserId is the primary participant (always a user/candidate).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(chat => chat.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // SecondUserId is the other user in a user-to-user chat (nullable).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(chat => chat.SecondUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // CompanyId is set for user-to-company chats (nullable).
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(chat => chat.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // JobId links the chat to a specific job posting (nullable).
        builder.HasOne<Job>()
            .WithMany()
            .HasForeignKey(chat => chat.JobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
