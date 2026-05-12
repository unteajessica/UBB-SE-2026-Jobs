using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private const int MaxFirstNameLength = 100;
    private const int MaxLastNameLength = 100;
    private const int MaxGenderLength = 20;
    private const int MaxEmailLength = 256;
    private const int MaxPhoneLength = 40;
    private const int MaxCountryLength = 100;
    private const int MaxCityLength = 100;
    private const int MaxAddressLength = 256;
    private const int MaxUniversityLength = 200;
    private const int MaxDegreeLength = 200;
    private const int MaxGitHubLength = 256;
    private const int MaxLinkedInLength = 256;
    private const int MaxMotivationLength = 2000;
    private const int MaxProfilePicturePathLength = 512;
    private const int MaxPreferredEmploymentTypeLength = 40;
    private const int MaxWorkModePreferenceLength = 40;
    private const int MaxLocationPreferenceLength = 100;
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.UserId);

        builder.Property(user => user.FirstName).HasMaxLength(MaxFirstNameLength).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(MaxLastNameLength).IsRequired();
        builder.Property(user => user.Gender).HasMaxLength(MaxGenderLength);
        builder.Property(user => user.Email).HasMaxLength(MaxEmailLength).IsRequired();
        builder.Property(user => user.Phone).HasMaxLength(MaxPhoneLength);
        builder.Property(user => user.Country).HasMaxLength(MaxCountryLength);
        builder.Property(user => user.City).HasMaxLength(MaxCityLength);
        builder.Property(user => user.Address).HasMaxLength(MaxAddressLength);
        builder.Property(user => user.University).HasMaxLength(MaxUniversityLength);
        builder.Property(user => user.Degree).HasMaxLength(MaxDegreeLength);
        builder.Property(user => user.GitHub).HasMaxLength(MaxGitHubLength);
        builder.Property(user => user.LinkedIn).HasMaxLength(MaxLinkedInLength);
        builder.Property(user => user.Motivation).HasMaxLength(MaxMotivationLength);
        builder.Property(user => user.ProfilePicturePath).HasMaxLength(MaxProfilePicturePathLength);
        builder.Property(user => user.ParsedCv).HasColumnType("nvarchar(max)");
        builder.Property(user => user.PreferredEmploymentType).HasMaxLength(MaxPreferredEmploymentTypeLength);
        builder.Property(user => user.WorkModePreference).HasMaxLength(MaxWorkModePreferenceLength);
        builder.Property(user => user.LocationPreference).HasMaxLength(MaxLocationPreferenceLength);
        // Email is the natural login identifier — must be unique across the catalog.
        builder.HasIndex(user => user.Email).IsUnique();

        // Cascade: deleting a user wipes their owned profile data (per MergePlan §4 and the
        // cascade decision in Phase 2). Match relationships are restricted in MatchConfiguration
        // so historical match records survive a user removal — that is intentional, callers must
        // soft-delete via ActiveAccount = false instead.
        builder.HasMany(user => user.WorkExperiences)
            .WithOne(workExperience => workExperience.User)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Projects)
            .WithOne(project => project.User)
            .HasForeignKey(project => project.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.ExtraCurricularActivities)
            .WithOne(activity => activity.User)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Skills)
            .WithOne(skill => skill.User)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // PersonalityResult is one-to-zero-or-one; cascade so the result and its trait scores
        // disappear when the user does.
        builder.HasOne(user => user.PersonalityResult)
            .WithOne(result => result.User)
            .HasForeignKey<PersonalityTestResult>("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Match navigation is configured from the Match side (restrict). Avoid configuring the
        // back-reference here to prevent EF from emitting two cascade paths.
        builder.HasData(
            new User { UserId = 1, FirstName = "Alice", LastName = "Smith", Email = "alice.smith@example.com" , Age = 25, Phone="+40123456789", Country="Romania", City="Bucharest", Address="123 Main St", University="University of Bucharest", Degree="Computer Science", UniversityStartYear=2018, ExpectedGraduationYear=2022, GitHub= "", ActiveAccount= true, CreatedAt = new DateTime(2025, 5, 7) }
        );
    }
}
