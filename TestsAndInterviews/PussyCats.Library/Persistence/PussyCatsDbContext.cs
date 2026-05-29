using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Persistence;

public class PussyCatsDbContext : DbContext
{
    public PussyCatsDbContext(DbContextOptions<PussyCatsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<PersonalityTestResult> PersonalityTestResults => Set<PersonalityTestResult>();
    public DbSet<SkillGroup> SkillGroups => Set<SkillGroup>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<SkillTest> SkillTests => Set<SkillTest>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PussyCatsDbContext).Assembly);
    }
}
