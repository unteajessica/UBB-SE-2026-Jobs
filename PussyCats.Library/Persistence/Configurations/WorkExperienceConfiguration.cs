using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class WorkExperienceConfiguration : IEntityTypeConfiguration<WorkExperience>
{
    public void Configure(EntityTypeBuilder<WorkExperience> builder)
    {
        builder.ToTable("WorkExperiences");
        builder.HasKey(workExperience => workExperience.WorkExperienceId);

        builder.Property(workExperience => workExperience.Company).HasMaxLength(200);
        builder.Property(workExperience => workExperience.JobTitle).HasMaxLength(200);
        builder.Property(workExperience => workExperience.Description).HasMaxLength(2000);

        // Cascade is configured on UserConfiguration (User -> WorkExperiences). Index supports
        // the GetByUserId-style queries used during profile loads.
        builder.HasIndex(workExperience => workExperience.UserId);
    }
}
