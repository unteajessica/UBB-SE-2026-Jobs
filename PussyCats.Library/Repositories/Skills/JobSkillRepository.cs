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
    public async Task<JobSkill?> GetAsync(int jobId, int skillId, CancellationToken ct = default)
    {
        return await db.JobSkills
            .Include(s => s.Skill)
            .FirstOrDefaultAsync(s => s.JobId == jobId && s.SkillId == skillId, ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.JobSkills
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: matchmaking JobSkillRepository.GetByJobId — straight predicate port. Read-only,
    /// includes Skill.
    /// </summary>
    public async Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken ct = default)
    {
        return await db.JobSkills
            .AsNoTracking()
            .Include(s => s.Skill)
            .Where(s => s.JobId == jobId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken ct = default)
    {
        db.JobSkills.Add(jobSkill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return jobSkill;
    }

    public async Task UpdateAsync(JobSkill jobSkill, CancellationToken ct = default)
    {
        db.JobSkills.Update(jobSkill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, int skillId, CancellationToken ct = default)
    {
        var jobSkill = await db.JobSkills.FindAsync(new object?[] { jobId, skillId }, ct).ConfigureAwait(false);
        if (jobSkill is null)
        {
            return;
        }
        db.JobSkills.Remove(jobSkill);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
