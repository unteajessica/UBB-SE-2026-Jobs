using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Skills;

public class JobSkillRepository : IJobSkillRepository
{
    private readonly PussyCatsDbContext db;

    public JobSkillRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Tracked single-row lookup keyed by (JobId, SkillId). Includes Skill so the caller can
    /// render the catalog name without a second query.
    /// </summary>
    public async Task<JobSkill?> GetAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        return await db.JobSkills
            .Include(skill => skill.Skill)
            .FirstOrDefaultAsync(skill => skill.JobId == jobId && skill.SkillId == skillId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.JobSkills
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
        return await db.JobSkills
            .AsNoTracking()
            .Include(skill => skill.Skill)
            .Where(skill => skill.JobId == jobId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        db.JobSkills.Add(jobSkill);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return jobSkill;
    }

    public async Task UpdateAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        db.JobSkills.Update(jobSkill);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        var jobSkill = await db.JobSkills.FindAsync(new object?[] { jobId, skillId }, cancellationToken).ConfigureAwait(false);
        if (jobSkill is null)
        {
            return;
        }
        db.JobSkills.Remove(jobSkill);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
