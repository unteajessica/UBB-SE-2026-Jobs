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
        builder.HasKey(job => job.JobId);

        builder.Property(job => job.JobId).HasColumnName("job_id");
        builder.Property(job => job.CompanyId).HasColumnName("company_id");
        builder.Property(job => job.Photo).HasColumnName("photo").HasColumnType("nvarchar(max)");
        builder.Property(job => job.JobTitle).HasColumnName("job_title").HasMaxLength(255).IsRequired();
        builder.Property(job => job.IndustryField).HasColumnName("industry_field").HasMaxLength(255);
        builder.Property(job => job.JobType).HasColumnName("job_type").HasMaxLength(255);
        builder.Property(job => job.ExperienceLevel).HasColumnName("experience_level").HasMaxLength(255);
        builder.Property(job => job.StartDate).HasColumnName("start_date").HasColumnType("date");
        builder.Property(job => job.EndDate).HasColumnName("end_date").HasColumnType("date");
        builder.Property(job => job.JobDescription).HasColumnName("job_description").HasColumnType("nvarchar(max)");
        builder.Property(job => job.JobLocation).HasColumnName("job_location").HasMaxLength(255);
        builder.Property(job => job.AvailablePositions).HasColumnName("available_positions");
        builder.Property(job => job.PostedAt).HasColumnName("posted_at").HasColumnType("datetime");
        builder.Property(job => job.Salary).HasColumnName("salary");
        builder.Property(job => job.AmountPayed).HasColumnName("amount_payed");
        builder.Property(job => job.Deadline).HasColumnName("deadline").HasColumnType("date");
        builder.Property(job => job.PromotionLevel).HasColumnName("promotion_level");
        builder.Property(job => job.JobRole).HasColumnName("job_role");

        builder.Ignore(job => job.Location);
        builder.Ignore(job => job.EmploymentType);

        builder.HasOne(job => job.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(job => job.CompanyId);
        builder.HasIndex(job => job.JobLocation);
        builder.HasIndex(job => job.JobType);

        builder.HasData(
            new { JobId = 101, CompanyId = 1, JobTitle = "Backend C# Developer", IndustryField = "IT", JobType = "Full-time", ExperienceLevel = "Mid-Level", JobDescription = "Develop robust REST APIs using .NET Core.", JobLocation = "Remote", AvailablePositions = 3, Salary = (int?)95000, AmountPayed = (int?)0, PostedAt = (DateTime?)new DateTime(2026, 4, 15, 9, 0, 0), Deadline = (DateTime?)new DateTime(2026, 5, 15), PromotionLevel = (int?)2, JobRole = (JobRole?)JobRole.BackendDeveloper },
            new { JobId = 102, CompanyId = 2, JobTitle = "Data Engineer", IndustryField = "Data Science", JobType = "Contract", ExperienceLevel = "Senior", JobDescription = "Maintain cloud data pipelines and warehouses.", JobLocation = "New York, NY", AvailablePositions = 1, Salary = (int?)120000, AmountPayed = (int?)0, PostedAt = (DateTime?)new DateTime(2026, 4, 18, 10, 30, 0), Deadline = (DateTime?)new DateTime(2026, 6, 1), PromotionLevel = (int?)1, JobRole = (JobRole?)JobRole.DataAnalyst }
        );
    }
}
