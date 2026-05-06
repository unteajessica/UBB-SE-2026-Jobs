using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class PersonalityTestResultConfiguration : IEntityTypeConfiguration<PersonalityTestResult>
{
    public void Configure(EntityTypeBuilder<PersonalityTestResult> builder)
    {
        builder.ToTable("PersonalityTestResults");
        builder.HasKey(r => r.PersonalityTestResultId);

        // Nullable: null means "test taken but role not yet selected / not taken."
        // Stored as int by EF convention; no HasConversion needed.
        builder.Property(r => r.SelectedRole).IsRequired(false);

        // Cascade: deleting the result removes its trait scores. The User -> PersonalityResult
        // cascade is configured on UserConfiguration.
        builder.HasMany(r => r.TraitScores)
            .WithOne(skill => skill.PersonalityTestResult)
            .HasForeignKey(skill => skill.PersonalityTestResultId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserId is the natural lookup column (GetByUserIdAsync). The User -> PersonalityResult
        // relationship is one-to-zero-or-one, so the FK is unique.
        builder.HasIndex(r => r.UserId).IsUnique();
    }
}
