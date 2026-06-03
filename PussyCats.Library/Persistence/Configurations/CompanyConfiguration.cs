using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    const int MaxCompanyNameLength = 255;
    const int MaxCompanyLocationLength = 300;
    const int MaxCompanyEmailLength = 100;

    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(company => company.CompanyId);

        builder.Property(company => company.CompanyId).HasColumnName("company_id");
        builder.Property(company => company.Name).HasColumnName("company_name").HasMaxLength(MaxCompanyNameLength).IsRequired();
        builder.Property(company => company.AboutUs).HasColumnName("about_us").HasColumnType("nvarchar(max)");
        builder.Property(company => company.ProfilePictureUrl).HasColumnName("profile_picture_url").HasColumnType("nvarchar(max)");
        builder.Property(company => company.LogoUrl).HasColumnName("logo_picture_url").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Location).HasColumnName("location").HasMaxLength(MaxCompanyLocationLength);
        builder.Property(company => company.Email).HasColumnName("email").HasMaxLength(MaxCompanyEmailLength);
        builder.Property(company => company.PostedJobsCount).HasColumnName("posted_jobs_count");
        builder.Property(company => company.CollaboratorsCount).HasColumnName("collaborators_count");

        builder.Ignore(company => company.CompanyName);
        builder.Ignore(company => company.LogoText);
        builder.Ignore(company => company.Phone);

        builder.Property(company => company.BuddyName).HasColumnName("buddy_name").HasMaxLength(255);
        builder.Property(company => company.AvatarId).HasColumnName("avatar_id");
        builder.Property(company => company.FinalQuote).HasColumnName("final_quote").HasColumnType("nvarchar(max)");
        builder.Property(company => company.BuddyDescription).HasColumnName("buddy_description").HasMaxLength(255);

        builder.Property(company => company.Scen1Text).HasColumnName("scen_1_text").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen1Answer1).HasColumnName("scen1_answer1").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen1Answer2).HasColumnName("scen1_answer2").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen1Answer3).HasColumnName("scen1_answer3").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen1Reaction1).HasColumnName("scen1_reaction1").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen1Reaction2).HasColumnName("scen1_reaction2").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen1Reaction3).HasColumnName("scen1_reaction3").HasColumnType("nvarchar(max)");

        builder.Property(company => company.Scen2Text).HasColumnName("scen2_text").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen2Answer1).HasColumnName("scen2_answer1").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen2Answer2).HasColumnName("scen2_answer2").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen2Answer3).HasColumnName("scen2_answer3").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen2Reaction1).HasColumnName("scen2_reaction1").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen2Reaction2).HasColumnName("scen2_reaction2").HasColumnType("nvarchar(max)");
        builder.Property(company => company.Scen2Reaction3).HasColumnName("scen2_reaction3").HasColumnType("nvarchar(max)");

        builder.HasData(
            new Company { CompanyId = 1, Name = "TechNova", Email = "hr@technova.com", LogoUrl = "technova_logo.png", Location = "San Francisco, CA", PostedJobsCount = 1, CollaboratorsCount = 1 },
            new Company { CompanyId = 2, Name = "DataFlow Inc", Email = "careers@dataflow.com", LogoUrl = "dataflow_logo.png", Location = "New York, NY", PostedJobsCount = 1, CollaboratorsCount = 1 },
            new Company { CompanyId = 3, Name = "EcoCode", Email = "hello@ecocode.com", LogoUrl = "ecocode_logo.png", Location = "Seattle, WA", PostedJobsCount = 0, CollaboratorsCount = 2 },
            new Company { CompanyId = 4, Name = "FinEdge", Email = "hr@finedge.com", LogoUrl = "finedge_logo.png", Location = "London, UK", PostedJobsCount = 0, CollaboratorsCount = 1 });
    }
}
