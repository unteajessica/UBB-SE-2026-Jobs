using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Jobs;

public class JobRepository : IJobRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public JobRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <summary>
    /// Includes Company and RequiredSkills.Skill so a job-detail screen has everything it needs
    /// to render. Tracked because the typical caller (recruiter editing a posting) mutates.
    /// </summary>
    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Jobs
            .Include(job => job.Company)
            .Include(job => job.RequiredSkills).ThenInclude(skill => skill.Skill)
            .FirstOrDefaultAsync(job => job.JobId == jobId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Browse-jobs listing — includes Company so the listing card can show the employer name
    /// without an N+1.
    /// </summary>
    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Jobs
            .AsNoTracking()
            .Include(job => job.Company)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: matchmaking JobRepository.GetByCompanyId — straight LINQ port of the foreach
    /// filter on CompanyId. Read-only, no Includes (callers already have the Company).
    /// </summary>
    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Jobs
            .AsNoTracking()
            .Where(job => job.Company.CompanyId == companyId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        databaseContext.Jobs.Add(job);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return job;
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        var tracked = databaseContext.Jobs.Local.FirstOrDefault(existing => existing.JobId == job.JobId);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(job);
        }
        else
        {
            databaseContext.Entry(job).State = EntityState.Modified;
        }
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var job = await databaseContext.Jobs.FindAsync(new object?[] { jobId }, cancellationToken).ConfigureAwait(false);
        if (job is null)
        {
            return;
        }
        databaseContext.Jobs.Remove(job);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
