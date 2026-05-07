using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(skill => skill.SkillId);

        builder.Property(skill => skill.Name).HasMaxLength(100).IsRequired();
        builder.Property(skill => skill.Category).HasMaxLength(100);

        // Catalog uniqueness — adding "C#" twice is a bug, never a feature.
        builder.HasIndex(skill => skill.Name).IsUnique();

        // Restrict on Skill -> UserSkill, JobSkill, SkillGroup. The catalog is foundational; you
        // shouldn't be able to delete a skill that's currently in use anywhere. To retire a skill
        // we'd add a soft-delete IsActive flag in a future phase.
        // (UserSkill / JobSkill restrict relationships are configured on their own files; the
        // SkillGroup <-> Skill many-to-many uses EF defaults per the user's instruction.)

        // Catalog seed — IDs 1..32 are aligned with matchmaking's SkillRepository so its
        // user-skill seed data continues to make sense after the merge. IDs 33+ are skills that
        // appear only in PussyCats's SkillGroup definitions and need stable IDs so the SkillGroup
        // join can wire up by ID.
        builder.HasData(SkillCatalog.Seed);
    }
}
