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
    }
}
