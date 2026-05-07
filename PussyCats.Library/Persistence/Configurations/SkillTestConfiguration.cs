using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class SkillTestConfiguration : IEntityTypeConfiguration<SkillTest>
{
    public void Configure(EntityTypeBuilder<SkillTest> builder)
    {
        builder.ToTable("SkillTests");
        builder.HasKey(test => test.SkillTestId);

        builder.Property(test => test.Name).HasMaxLength(200).IsRequired();

        // Cascade: a user's skill test attempts are owned by the user — delete-the-user
        // wipes them.
        builder.HasOne(test => test.User)
            .WithMany()
            .HasForeignKey(test => test.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(test => test.UserId);

        // Demo data: three skill tests for User #1 so the Skill Tests page renders
        // without a runtime mutate-on-read seed. Dates are static for migration
        // determinism. Replaces the deleted SkillTestDefaults helper.
        var seededDate = new DateOnly(2026, 1, 7);
        builder.HasData(
            new SkillTest { SkillTestId = 1, UserId = 1, Name = "C# Fundamentals", Score = 82, AchievedDate = seededDate },
            new SkillTest { SkillTestId = 2, UserId = 1, Name = "SQL Server",       Score = 76, AchievedDate = seededDate },
            new SkillTest { SkillTestId = 3, UserId = 1, Name = "Software Design",  Score = 88, AchievedDate = seededDate });
    }
}
