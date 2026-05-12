using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class JobSkillConfiguration : IEntityTypeConfiguration<JobSkill>
{
    public void Configure(EntityTypeBuilder<JobSkill> builder)
    {
        builder.ToTable("JobSkills");

        // Composite PK (JobId, SkillId) — same rationale as UserSkill.
        builder.HasKey("JobId", "SkillId");

        // Cascade on Job -> JobSkill: a removed job has no remaining requirements.
        builder.HasOne(skill => skill.Job)
            .WithMany(job => job.RequiredSkills)
            .HasForeignKey("JobId")
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict on Skill -> JobSkill: catalog reference, can't be dropped while in use.
        builder.HasOne(skill => skill.Skill)
            .WithMany()
            .HasForeignKey("SkillId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("SkillId");

        // Three sample required-skill rows for the three seeded jobs.
        builder.HasData(
             new { JobId = 1, SkillId = 1, RequiredLevel = 80 },
             new { JobId = 1, SkillId = 3, RequiredLevel = 75 },
             new { JobId = 2, SkillId = 2, RequiredLevel = 70 },
             new { JobId = 2, SkillId = 12, RequiredLevel = 45 },
             new { JobId = 3, SkillId = 8, RequiredLevel = 68 },
             new { JobId = 3, SkillId = 9, RequiredLevel = 62 });
    }
}
