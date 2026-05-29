using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(c => c.CompanyId);

        builder.Property(c => c.CompanyId).HasColumnName("company_id");
        builder.Property(c => c.Name).HasColumnName("company_name").HasMaxLength(255).IsRequired();
        builder.Property(c => c.AboutUs).HasColumnName("about_us").HasColumnType("nvarchar(max)");
        builder.Property(c => c.ProfilePictureUrl).HasColumnName("profile_picture_url").HasColumnType("nvarchar(max)");
        builder.Property(c => c.LogoUrl).HasColumnName("logo_picture_url").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Location).HasColumnName("location").HasMaxLength(300);
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(100);
        builder.Property(c => c.PostedJobsCount).HasColumnName("posted_jobs_count");
        builder.Property(c => c.CollaboratorsCount).HasColumnName("collaborators_count");

        builder.Ignore(c => c.CompanyName);
        builder.Ignore(c => c.LogoText);
        builder.Ignore(c => c.Phone);

        builder.Property(c => c.BuddyName).HasColumnName("buddy_name").HasMaxLength(255);
        builder.Property(c => c.AvatarId).HasColumnName("avatar_id");
        builder.Property(c => c.FinalQuote).HasColumnName("final_quote").HasColumnType("nvarchar(max)");
        builder.Property(c => c.BuddyDescription).HasColumnName("buddy_description").HasMaxLength(255);

        builder.Property(c => c.Scen1Text).HasColumnName("scen_1_text").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen1Answer1).HasColumnName("scen1_answer1").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen1Answer2).HasColumnName("scen1_answer2").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen1Answer3).HasColumnName("scen1_answer3").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen1Reaction1).HasColumnName("scen1_reaction1").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen1Reaction2).HasColumnName("scen1_reaction2").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen1Reaction3).HasColumnName("scen1_reaction3").HasColumnType("nvarchar(max)");

        builder.Property(c => c.Scen2Text).HasColumnName("scen2_text").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen2Answer1).HasColumnName("scen2_answer1").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen2Answer2).HasColumnName("scen2_answer2").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen2Answer3).HasColumnName("scen2_answer3").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen2Reaction1).HasColumnName("scen2_reaction1").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen2Reaction2).HasColumnName("scen2_reaction2").HasColumnType("nvarchar(max)");
        builder.Property(c => c.Scen2Reaction3).HasColumnName("scen2_reaction3").HasColumnType("nvarchar(max)");

        builder.HasData(
            new Company { CompanyId = 1, Name = "TechNova", Email = "hr@technova.com", LogoUrl = "technova_logo.png", Location = "San Francisco, CA", PostedJobsCount = 1, CollaboratorsCount = 1 },
            new Company { CompanyId = 2, Name = "DataFlow Inc", Email = "careers@dataflow.com", LogoUrl = "dataflow_logo.png", Location = "New York, NY", PostedJobsCount = 1, CollaboratorsCount = 1 },
            new Company { CompanyId = 3, Name = "EcoCode", Email = "hello@ecocode.com", LogoUrl = "ecocode_logo.png", Location = "Seattle, WA", PostedJobsCount = 0, CollaboratorsCount = 2 },
            new Company { CompanyId = 4, Name = "FinEdge", Email = "hr@finedge.com", LogoUrl = "finedge_logo.png", Location = "London, UK", PostedJobsCount = 0, CollaboratorsCount = 1 });
    }
}
