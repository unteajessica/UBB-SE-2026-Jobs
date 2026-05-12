using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class PersonalityTestResultConfiguration : IEntityTypeConfiguration<PersonalityTestResult>
{
    public void Configure(EntityTypeBuilder<PersonalityTestResult> builder)
    {
        builder.ToTable("PersonalityTestResults");
        builder.HasKey(result => result.PersonalityTestResultId);

        // Nullable: null means "test taken but role not yet selected / not taken."
        // Stored as int by EF convention; no HasConversion needed.
        builder.Property(result => result.SelectedRole).IsRequired(false);

        // Cascade: deleting the result removes its trait scores. The User -> PersonalityResult
        // cascade is configured on UserConfiguration.
        builder.HasMany(result => result.TraitScores)
            .WithOne(skill => skill.PersonalityTestResult)
            .HasForeignKey(skill => skill.PersonalityTestResultId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserId is the natural lookup column (GetByUserIdAsync). The User -> PersonalityResult
        // relationship is one-to-zero-or-one, so the FK is unique.
        builder.HasIndex("UserId").IsUnique();
    }
}
