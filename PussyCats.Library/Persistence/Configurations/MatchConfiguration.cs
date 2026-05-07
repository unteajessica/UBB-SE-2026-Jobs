using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(match => match.MatchId);

        builder.Property(match => match.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(match => match.FeedbackMessage).HasMaxLength(2000);

        // Restrict on User -> Match. Match history may still be referenced by the company side
        // (applicant lists, audit) and silently destroying it on user removal is the wrong move.
        // To remove a user, callers soft-delete via ActiveAccount = false. Match history stays.
        builder.HasOne(match => match.User)
            .WithMany(user => user.Matches)
            .HasForeignKey(match => match.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict on Job -> Match for the same reasoning: a deleted/archived job should not
        // erase the application records of every applicant.
        builder.HasOne(match => match.Job)
            .WithMany(job => job.Matches)
            .HasForeignKey(match => match.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes per spec — JobId stands alone (joins from the Job side); Status drives the
        // GET /api/users/{id}/matches?status= filter.
        builder.HasIndex(match => match.JobId);
        builder.HasIndex(match => match.Status);
        // Composite (UserId, JobId) supports GetByUserIdAndJobIdAsync (hot path in MatchService)
        // and also covers UserId-only queries via the leftmost-prefix rule, so a standalone
        // IX_Matches_UserId would be redundant.
        builder.HasIndex(match => new { match.UserId, match.JobId });
    }
}
