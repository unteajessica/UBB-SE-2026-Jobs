using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.UserId);

        builder.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.Gender).HasMaxLength(20);
        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
        builder.Property(user => user.Phone).HasMaxLength(40);
        builder.Property(user => user.Country).HasMaxLength(100);
        builder.Property(user => user.City).HasMaxLength(100);
        builder.Property(user => user.Address).HasMaxLength(256);
        builder.Property(user => user.University).HasMaxLength(200);
        builder.Property(user => user.Degree).HasMaxLength(200);
        builder.Property(user => user.GitHub).HasMaxLength(256);
        builder.Property(user => user.LinkedIn).HasMaxLength(256);
        builder.Property(user => user.Motivation).HasMaxLength(2000);
        builder.Property(user => user.ProfilePicturePath).HasMaxLength(512);
        builder.Property(user => user.ParsedCv).HasColumnType("nvarchar(max)");
        builder.Property(user => user.PreferredEmploymentType).HasMaxLength(40);
        builder.Property(user => user.WorkModePreference).HasMaxLength(40);
        builder.Property(user => user.LocationPreference).HasMaxLength(100);
        // Email is the natural login identifier — must be unique across the catalog.
        builder.HasIndex(user => user.Email).IsUnique();

        // Cascade: deleting a user wipes their owned profile data (per MergePlan §4 and the
        // cascade decision in Phase 2). Match relationships are restricted in MatchConfiguration
        // so historical match records survive a user removal — that is intentional, callers must
        // soft-delete via ActiveAccount = false instead.
        builder.HasMany(user => user.WorkExperiences)
            .WithOne(workExperience => workExperience.User)
            .HasForeignKey(workExperience => workExperience.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Projects)
            .WithOne(project => project.User)
            .HasForeignKey(project => project.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.ExtraCurricularActivities)
            .WithOne(activity => activity.User)
            .HasForeignKey(activity => activity.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Skills)
            .WithOne(skill => skill.User)
            .HasForeignKey(skill => skill.UserId)
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
