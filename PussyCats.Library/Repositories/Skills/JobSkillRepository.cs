using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Skills;

public class JobSkillRepository : IJobSkillRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public JobSkillRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <summary>
    /// Tracked single-row lookup keyed by (JobId, SkillId). Includes Skill so the caller can
    /// render the catalog name without a second query.
    /// </summary>
    public async Task<JobSkill?> GetAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.JobSkills
            .Include(skill => skill.Skill)
            .FirstOrDefaultAsync(skill => skill.JobId == jobId && skill.SkillId == skillId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.JobSkills
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: matchmaking JobSkillRepository.GetByJobId — straight predicate port. Read-only,
    /// includes Skill.
    /// </summary>
    public async Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.JobSkills
            .AsNoTracking()
            .Include(skill => skill.Skill)
            .Where(skill => skill.JobId == jobId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        databaseContext.JobSkills.Add(jobSkill);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return jobSkill;
    }

    public async Task UpdateAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        var tracked = databaseContext.JobSkills.Local.FirstOrDefault(existing => existing.JobId == jobSkill.JobId && existing.SkillId == jobSkill.SkillId);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(jobSkill);
        }
        else
        {
            databaseContext.Entry(jobSkill).State = EntityState.Modified;
        }
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        var jobSkill = await databaseContext.JobSkills.FindAsync(new object?[] { jobId, skillId }, cancellationToken).ConfigureAwait(false);
        if (jobSkill is null)
        {
            return;
        }
        databaseContext.JobSkills.Remove(jobSkill);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
