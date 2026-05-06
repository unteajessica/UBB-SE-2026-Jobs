using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Jobs;

public class JobRepository : IJobRepository
{
    private readonly PussyCatsDbContext db;

    public JobRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Includes Company and RequiredSkills.Skill so a job-detail screen has everything it needs
    /// to render. Tracked because the typical caller (recruiter editing a posting) mutates.
    /// </summary>
    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await db.Jobs
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
        return await db.Jobs
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
        return await db.Jobs
            .AsNoTracking()
            .Where(job => job.CompanyId == companyId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        db.Jobs.Add(job);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return job;
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        db.Jobs.Update(job);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var job = await db.Jobs.FindAsync(new object?[] { jobId }, cancellationToken).ConfigureAwait(false);
        if (job is null)
        {
            return;
        }
        db.Jobs.Remove(job);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
