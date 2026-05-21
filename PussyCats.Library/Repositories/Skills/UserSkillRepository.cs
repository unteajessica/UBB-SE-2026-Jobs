using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Skills;

public class UserSkillRepository : IUserSkillRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public UserSkillRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <summary>
    /// Tracked single-row lookup keyed by the (UserId, SkillId) composite. Includes Skill so
    /// the caller can render the catalog name without a second query.
    /// </summary>
    public async Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.UserSkills
            .Include(skill => skill.Skill)
            .Include(skill => skill.User)
            .FirstOrDefaultAsync(skill => skill.User.UserId == userId && skill.Skill.SkillId == skillId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Read-only listing of every claimed skill for a user. Includes Skill for catalog name.
    /// </summary>
    public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.UserSkills
            .AsNoTracking()
            .Include(skill => skill.Skill)
            .Include(skill => skill.User)
            .Where(skill => skill.User.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp UserSkillRepository.GetVerifiedSkillsByUserId — the original SQL
    /// only filtered on userId and *implied* IsVerified by reading the SKILLS table (the legacy
    /// schema only stored verified rows there). The new model keeps unverified self-claims in
    /// the same UserSkill table, so the LINQ predicate also requires IsVerified = true and
    /// AchievedDate IS NOT NULL — both must hold for a skill to count as "verified", per the
    /// AchievedDate XML documentation note on UserSkill.
    /// </summary>
    public async Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.UserSkills
            .AsNoTracking()
            .Include(skill => skill.Skill)
            .Include(skill => skill.User)
            .Where(skill => skill.User.UserId == userId && skill.IsVerified && skill.AchievedDate != null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        //databaseContext.UserSkills.Add(userSkill);
        //await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        //return userSkill;
        databaseContext.Entry(userSkill.User).State = EntityState.Unchanged;
        databaseContext.Entry(userSkill.Skill).State = EntityState.Unchanged;
        databaseContext.UserSkills.Add(userSkill);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return userSkill;
    }

    public async Task UpdateAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        var tracked = databaseContext.UserSkills.Local.FirstOrDefault(existing => existing.User.UserId == userSkill.User.UserId && existing.Skill.SkillId == userSkill.Skill.SkillId);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(userSkill);
        }
        else
        {
            databaseContext.Entry(userSkill).State = EntityState.Modified;
        }
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
    public async Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken cancellationToken = default)
    {
        var userSkill = await databaseContext.UserSkills
            .FirstOrDefaultAsync(us => EF.Property<int>(us, "UserId") == userId
                                    && EF.Property<int>(us, "SkillId") == skillId, cancellationToken)
            .ConfigureAwait(false);

        if (userSkill is null) return;
        userSkill.Score = score;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        var userSkill = await databaseContext.UserSkills
            .FirstOrDefaultAsync(us => EF.Property<int>(us, "UserId") == userId
                                    && EF.Property<int>(us, "SkillId") == skillId, cancellationToken)
            .ConfigureAwait(false);

        if (userSkill is null) return;
        databaseContext.UserSkills.Remove(userSkill);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

}
