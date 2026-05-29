using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Skills;

public class SkillRepository : ISkillRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public SkillRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    public async Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Skills
            .FirstOrDefaultAsync(skill => skill.SkillId == skillId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Catalog listing — read-only, ordered by name for stable UI rendering.
    /// </summary>
    public async Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Skills
            .AsNoTracking()
            .OrderBy(skill => skill.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        databaseContext.Skills.Add(skill);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return skill;
    }

    public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        var tracked = databaseContext.Skills.Local.FirstOrDefault(existing => existing.SkillId == skill.SkillId);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(skill);
        }
        else
        {
            databaseContext.Entry(skill).State = EntityState.Modified;
        }
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int skillId, CancellationToken cancellationToken = default)
    {
        var isReferencedByUser = await databaseContext.UserSkills
            .AnyAsync(skill => EF.Property<int>(skill, "SkillId") == skillId, cancellationToken)
            .ConfigureAwait(false);

        if (isReferencedByUser)
        {
            throw new InvalidOperationException("Skill is in use by one or more users and cannot be deleted.");
        }

        var isReferencedByJob = await databaseContext.JobSkills
            .AnyAsync(skill => EF.Property<int>(skill, "SkillId") == skillId, cancellationToken)
            .ConfigureAwait(false);

        if (isReferencedByJob)
        {
            throw new InvalidOperationException("Skill is required by one or more jobs and cannot be deleted.");
        }

        var skill = await databaseContext.Skills.FindAsync(new object?[] { skillId }, cancellationToken).ConfigureAwait(false);
        if (skill is null)
        {
            return;
        }
        databaseContext.Skills.Remove(skill);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
