using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    private const int MaxJobTitleLength = 200;
    private const int MaxJobDescriptionLength = 4000;
    private const int MaxLocationLength = 100;
    private const int MaxEmploymentTypeLength = 40;
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");
        builder.HasKey(job => job.JobId);

        builder.Property(job => job.JobTitle).HasMaxLength(MaxJobTitleLength).IsRequired();
        builder.Property(job => job.JobDescription).HasMaxLength(MaxJobDescriptionLength);
        builder.Property(job => job.Location).HasMaxLength(MaxLocationLength);
        builder.Property(job => job.EmploymentType).HasMaxLength(MaxEmploymentTypeLength);

        // Stored as int by EF convention; no HasConversion needed.
        builder.Property(job => job.JobRole);

        // Restrict: deleting a company should not nuke its job postings. Archive instead.
        builder.HasOne(job => job.Company)
            .WithMany(company => company.Jobs)
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Restrict);

        // JobSkill cascade: when a job is removed its skill requirements go too (configured on
        // JobSkillConfiguration). Match relationships are restricted (configured on
        // MatchConfiguration) so historical applications survive job archival.

        // Indexes for common API filters: GET /api/jobs?location=&type=
        builder.HasIndex(job => job.Location);
        builder.HasIndex(job => job.EmploymentType);
        // CompanyId is heavily filtered on (GetByCompanyIdAsync) — explicit non-unique index.
        builder.HasIndex("CompanyId");

        builder.HasData(
            new { JobId = 1, CompanyId = 1, JobTitle = "Backend .NET Developer", JobDescription = "Join our Bucharest team building enterprise APIs and integrations. Strong C# and SQL; experience with Azure or containers is a plus.", Location = "Bucharest", EmploymentType = "Hybrid", PromotionLevel = 2, JobRole = JobRole.BackendDeveloper },
            new { JobId = 2, CompanyId = 2, JobTitle = "Junior Frontend Developer", JobDescription = "Ship UI features for our web app under mentorship. Learn React, testing, and our design system while pairing with senior engineers.", Location = "Cluj-Napoca", EmploymentType = "Full-time", PromotionLevel = 1, JobRole = JobRole.FrontendDeveloper },
            new { JobId = 3, CompanyId = 3, JobTitle = "Data Analyst", JobDescription = "Turn business questions into dashboards and ad hoc analyses. SQL and visualization tools; curiosity about the domain.", Location = "Brasov", EmploymentType = "Hybrid", PromotionLevel = 1, JobRole = JobRole.DataAnalyst }
        );
       
    }
}
