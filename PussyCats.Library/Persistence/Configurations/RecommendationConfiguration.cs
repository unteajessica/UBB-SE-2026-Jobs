using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("Recommendations");
        builder.HasKey(recommendation => recommendation.RecommendationId);

        // Cascade: User -> Recommendation. Per-user recommendations are derived from the user's
        // own profile; once the user is gone there is nothing meaningful to keep.
        builder.HasOne(recommendation => recommendation.User)
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict on Job -> Recommendation. SQL Server forbids two cascade paths converging on
        // one table, and User -> Recommendation already cascades; restricting here also guards
        // against accidentally deleting recommendations across all users when a job is removed.
        builder.HasOne(recommendation => recommendation.Job)
            .WithMany()
            .HasForeignKey("JobId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("UserId");
        builder.HasIndex("JobId");
        // (UserId, JobId, Timestamp DESC) supports GetLatestByUserIdAndJobIdAsync without a sort.
        builder.HasIndex("UserId", "JobId", nameof(Recommendation.Timestamp));
    }
}
