using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");
        builder.HasKey(j => j.JobId);

        builder.Property(j => j.JobId).HasColumnName("job_id");
        builder.Property(j => j.CompanyId).HasColumnName("company_id");
        builder.Property(j => j.Photo).HasColumnName("photo").HasColumnType("nvarchar(max)");
        builder.Property(j => j.JobTitle).HasColumnName("job_title").HasMaxLength(255).IsRequired();
        builder.Property(j => j.IndustryField).HasColumnName("industry_field").HasMaxLength(255);
        builder.Property(j => j.JobType).HasColumnName("job_type").HasMaxLength(255);
        builder.Property(j => j.ExperienceLevel).HasColumnName("experience_level").HasMaxLength(255);
        builder.Property(j => j.StartDate).HasColumnName("start_date").HasColumnType("date");
        builder.Property(j => j.EndDate).HasColumnName("end_date").HasColumnType("date");
        builder.Property(j => j.JobDescription).HasColumnName("job_description").HasColumnType("nvarchar(max)");
        builder.Property(j => j.JobLocation).HasColumnName("job_location").HasMaxLength(255);
        builder.Property(j => j.AvailablePositions).HasColumnName("available_positions");
        builder.Property(j => j.PostedAt).HasColumnName("posted_at").HasColumnType("datetime");
        builder.Property(j => j.Salary).HasColumnName("salary");
        builder.Property(j => j.AmountPayed).HasColumnName("amount_payed");
        builder.Property(j => j.Deadline).HasColumnName("deadline").HasColumnType("date");
        builder.Property(j => j.PromotionLevel).HasColumnName("promotion_level");
        builder.Property(j => j.JobRole).HasColumnName("job_role");

        builder.Ignore(j => j.Location);
        builder.Ignore(j => j.EmploymentType);

        builder.HasOne(j => j.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(j => j.CompanyId);
        builder.HasIndex(j => j.JobLocation);
        builder.HasIndex(j => j.JobType);

        builder.HasData(
            new { JobId = 101, CompanyId = 1, JobTitle = "Backend C# Developer", IndustryField = "IT", JobType = "Full-time", ExperienceLevel = "Mid-Level", JobDescription = "Develop robust REST APIs using .NET Core.", JobLocation = "Remote", AvailablePositions = 3, Salary = (int?)95000, AmountPayed = (int?)0, PostedAt = (DateTime?)new DateTime(2026, 4, 15, 9, 0, 0), Deadline = (DateTime?)new DateTime(2026, 5, 15), PromotionLevel = (int?)2, JobRole = (JobRole?)JobRole.BackendDeveloper },
            new { JobId = 102, CompanyId = 2, JobTitle = "Data Engineer", IndustryField = "Data Science", JobType = "Contract", ExperienceLevel = "Senior", JobDescription = "Maintain cloud data pipelines and warehouses.", JobLocation = "New York, NY", AvailablePositions = 1, Salary = (int?)120000, AmountPayed = (int?)0, PostedAt = (DateTime?)new DateTime(2026, 4, 18, 10, 30, 0), Deadline = (DateTime?)new DateTime(2026, 6, 1), PromotionLevel = (int?)1, JobRole = (JobRole?)JobRole.DataAnalyst }
        );
    }
}
