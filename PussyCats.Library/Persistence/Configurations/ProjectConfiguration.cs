using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(project => project.ProjectId);

        builder.Property(project => project.Name).HasMaxLength(200);
        builder.Property(project => project.Description).HasMaxLength(2000);
        builder.Property(project => project.Url).HasMaxLength(512);

        // Technologies is a primitive collection; EF Core's primitive-collection support stores
        // it as a JSON column on the Projects table (no value converter, no join table).
        builder.PrimitiveCollection(project => project.Technologies)
            .HasColumnType("nvarchar(max)");

        // Cascade configured on UserConfiguration (User -> Projects).
        builder.HasIndex(project => project.UserId);
    }
}
