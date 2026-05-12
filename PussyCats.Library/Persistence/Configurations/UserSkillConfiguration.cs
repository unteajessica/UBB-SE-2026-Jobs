using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("UserSkills");

        // Composite natural key (UserId, SkillId) — there is no single surrogate id, and the
        // unique-composite-index requirement collapses naturally into the primary key.
        builder.HasKey("UserId", "SkillId");

        // Cascade on User -> UserSkill (configured on UserConfiguration). When a user is hard
        // deleted (rare — soft delete is preferred), their per-skill scores go with them.
        // Restrict on Skill -> UserSkill: the catalog is foundational, you can't drop a skill
        // that users still claim. Retire via IsActive instead in a future phase.
        builder.HasOne(skill => skill.Skill)
            .WithMany()
            .HasForeignKey("SkillId")
            .OnDelete(DeleteBehavior.Restrict);

        // Useful filters: (UserId) is covered by the PK; SkillId still needs its own index for
        // "who has this skill?" lookups (applicant search by skill).
        builder.HasIndex("SkillId");
    }
}
