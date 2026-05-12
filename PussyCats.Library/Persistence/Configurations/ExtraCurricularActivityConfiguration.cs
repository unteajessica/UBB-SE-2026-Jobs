using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class ExtraCurricularActivityConfiguration : IEntityTypeConfiguration<ExtraCurricularActivity>
{
    private const int MaxActivityNameLength = 200;
    private const int MaxOrganizationLength = 200;
    private const int MaxRoleNameLength = 200;
    private const int MaxActivityPeriodLength = 100;
    private const int MaxActivityDescriptionLength = 2000;
    public void Configure(EntityTypeBuilder<ExtraCurricularActivity> builder)
    {
        builder.ToTable("ExtraCurricularActivities");
        builder.HasKey(activity => activity.ExtraCurricularActivityId);

        builder.Property(activity => activity.ActivityName).HasMaxLength(MaxActivityNameLength);
        builder.Property(activity => activity.Organization).HasMaxLength(MaxOrganizationLength);
        builder.Property(activity => activity.Role).HasMaxLength(MaxRoleNameLength);
        builder.Property(activity => activity.Period).HasMaxLength(MaxActivityPeriodLength);
        builder.Property(activity => activity.Description).HasMaxLength(MaxActivityDescriptionLength);

        // Cascade configured on UserConfiguration (User -> ExtraCurricularActivities).
        builder.HasIndex("UserId");
    }
}
