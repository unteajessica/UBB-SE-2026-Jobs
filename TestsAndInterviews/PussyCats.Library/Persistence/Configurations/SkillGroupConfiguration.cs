using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class SkillGroupConfiguration : IEntityTypeConfiguration<SkillGroup>
{
    public void Configure(EntityTypeBuilder<SkillGroup> builder)
    {
        builder.ToTable("SkillGroups");
        builder.HasKey(group => group.SkillGroupId);

        builder.Property(group => group.GroupName).HasMaxLength(100).IsRequired();
        builder.Property(group => group.JobRole).HasConversion<string>().HasMaxLength(40);

        // Many-to-many SkillGroup <-> Skill through an EF-managed join table. The join is
        // explicit so the shadow-FK columns are SkillGroupId / SkillId (matching the seed's
        // anonymous-object shape) rather than EF's default "SkillsSkillId"-style convention.
        // SkillGroup -> join is Cascade: removing a group cleans up its membership rows.
        // Skill -> join is Restrict: the catalog is foundational; you can't drop a skill that
        // is still listed in any group. Same rule as Skill -> UserSkill / JobSkill — retire
        // skills via a soft-delete flag in a future phase, never by removing the row.
        builder.HasMany(group => group.Skills)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "SkillGroupSkills",
                right => right.HasOne<Skill>().WithMany().HasForeignKey("SkillId").OnDelete(DeleteBehavior.Restrict),
                left => left.HasOne<SkillGroup>().WithMany().HasForeignKey("SkillGroupId").OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("SkillGroupId", "SkillId");
                    join.ToTable("SkillGroupSkills");
                    join.HasData(SkillGroupSeed.Memberships);
                });

        // GetByJobRoleAsync filters on JobRole; index covers it.
        builder.HasIndex(group => group.JobRole);

        // Verbatim port of PussyCatsApp's SkillGroup definitions — names, weights, and member
        // skills. The weights were tuned by the original team and must not be touched.
        builder.HasData(SkillGroupSeed.Groups);
    }
}
