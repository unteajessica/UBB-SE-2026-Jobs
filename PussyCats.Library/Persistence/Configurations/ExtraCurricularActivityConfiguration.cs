using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class ExtraCurricularActivityConfiguration : IEntityTypeConfiguration<ExtraCurricularActivity>
{
    public void Configure(EntityTypeBuilder<ExtraCurricularActivity> builder)
    {
        builder.ToTable("ExtraCurricularActivities");
        builder.HasKey(activity => activity.ExtraCurricularActivityId);

        builder.Property(activity => activity.ActivityName).HasMaxLength(200);
        builder.Property(activity => activity.Organization).HasMaxLength(200);
        builder.Property(activity => activity.Role).HasMaxLength(200);
        builder.Property(activity => activity.Period).HasMaxLength(100);
        builder.Property(activity => activity.Description).HasMaxLength(2000);

        // Cascade configured on UserConfiguration (User -> ExtraCurricularActivities).
        builder.HasIndex(activity => activity.UserId);
    }
}
