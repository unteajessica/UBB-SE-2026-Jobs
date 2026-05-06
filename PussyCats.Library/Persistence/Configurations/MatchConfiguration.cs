using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(m => m.MatchId);

        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.FeedbackMessage).HasMaxLength(2000);

        // Restrict on User -> Match. Match history may still be referenced by the company side
        // (applicant lists, audit) and silently destroying it on user removal is the wrong move.
        // To remove a user, callers soft-delete via ActiveAccount = false. Match history stays.
        builder.HasOne(m => m.User)
            .WithMany(u => u.Matches)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict on Job -> Match for the same reasoning: a deleted/archived job should not
        // erase the application records of every applicant.
        builder.HasOne(m => m.Job)
            .WithMany(job => job.Matches)
            .HasForeignKey(m => m.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes per spec — JobId stands alone (joins from the Job side); Status drives the
        // GET /api/users/{id}/matches?status= filter.
        builder.HasIndex(m => m.JobId);
        builder.HasIndex(m => m.Status);
        // Composite (UserId, JobId) supports GetByUserIdAndJobIdAsync (hot path in MatchService)
        // and also covers UserId-only queries via the leftmost-prefix rule, so a standalone
        // IX_Matches_UserId would be redundant.
        builder.HasIndex(m => new { m.UserId, m.JobId });
    }
}
