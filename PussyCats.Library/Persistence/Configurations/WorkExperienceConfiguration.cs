using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class WorkExperienceConfiguration : IEntityTypeConfiguration<WorkExperience>
{
    private const int MaxCompanyLength = 200;
    private const int MaxJobTitleLength = 200;
    private const int MaxDescriptionLength = 2000;
    public void Configure(EntityTypeBuilder<WorkExperience> builder)
    {
        builder.ToTable("WorkExperiences");
        builder.HasKey(workExperience => workExperience.WorkExperienceId);

        builder.Property(workExperience => workExperience.Company).HasMaxLength(MaxCompanyLength);
        builder.Property(workExperience => workExperience.JobTitle).HasMaxLength(MaxJobTitleLength);
        builder.Property(workExperience => workExperience.Description).HasMaxLength(MaxDescriptionLength);

        // Cascade is configured on UserConfiguration (User -> WorkExperiences). Index supports
        // the GetByUserId-style queries used during profile loads.
        builder.HasIndex("UserId");
    }
}
