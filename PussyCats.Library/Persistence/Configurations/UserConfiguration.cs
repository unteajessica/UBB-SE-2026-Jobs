using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Gender).HasMaxLength(20);
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(40);
        builder.Property(u => u.Country).HasMaxLength(100);
        builder.Property(u => u.City).HasMaxLength(100);
        builder.Property(u => u.Address).HasMaxLength(256);
        builder.Property(u => u.University).HasMaxLength(200);
        builder.Property(u => u.Degree).HasMaxLength(200);
        builder.Property(u => u.GitHub).HasMaxLength(256);
        builder.Property(u => u.LinkedIn).HasMaxLength(256);
        builder.Property(u => u.Motivation).HasMaxLength(2000);
        builder.Property(u => u.ProfilePicturePath).HasMaxLength(512);
        builder.Property(u => u.ParsedCv).HasColumnType("nvarchar(max)");
        builder.Property(u => u.PreferredEmploymentType).HasMaxLength(40);
        builder.Property(u => u.WorkModePreference).HasMaxLength(40);
        builder.Property(u => u.LocationPreference).HasMaxLength(100);

        // Email is the natural login identifier — must be unique across the catalog.
        builder.HasIndex(u => u.Email).IsUnique();

        // Cascade: deleting a user wipes their owned profile data (per MergePlan §4 and the
        // cascade decision in Phase 2). Match relationships are restricted in MatchConfiguration
        // so historical match records survive a user removal — that is intentional, callers must
        // soft-delete via ActiveAccount = false instead.
        builder.HasMany(u => u.WorkExperiences)
            .WithOne(w => w.User)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Projects)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ExtraCurricularActivities)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Skills)
            .WithOne(skill => skill.User)
            .HasForeignKey(skill => skill.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // PersonalityResult is one-to-zero-or-one; cascade so the result and its trait scores
        // disappear when the user does.
        builder.HasOne(u => u.PersonalityResult)
            .WithOne(r => r.User)
            .HasForeignKey<PersonalityTestResult>(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Match navigation is configured from the Match side (restrict). Avoid configuring the
        // back-reference here to prevent EF from emitting two cascade paths.
    }
}
