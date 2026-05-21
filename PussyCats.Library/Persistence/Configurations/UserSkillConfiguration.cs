using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("UserSkills");

        // User side is already configured in UserConfiguration.HasMany(u => u.Skills)
        // Only configure the Skill side here
        builder.HasOne(us => us.Skill)
            .WithMany()
            .HasForeignKey("SkillId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey("UserId", "SkillId");

        builder.HasIndex("SkillId");
    }
}
