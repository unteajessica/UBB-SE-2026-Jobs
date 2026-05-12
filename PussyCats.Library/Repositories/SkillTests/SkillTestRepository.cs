using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.SkillTests;

public class SkillTestRepository : ISkillTestRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public SkillTestRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <summary>
    /// Tracked. Note: original PussyCatsApp SkillTestRepository.Load threw when the row was not
    /// found; the new contract returns null instead, matching the IXRepository nullable return
    /// shape and the §22 rule that not-found is not exceptional.
    /// </summary>
    public async Task<SkillTest?> GetByIdAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.SkillTests
            .FirstOrDefaultAsync(test => test.SkillTestId == skillTestId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp SkillTestRepository.GetSkillTestsByUserId — straight predicate
    /// port. Read-only.
    /// </summary>
    public async Task<IReadOnlyList<SkillTest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.SkillTests
            .AsNoTracking()
            .Where(test => test.User.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SkillTest> AddAsync(SkillTest skillTest, CancellationToken cancellationToken = default)
    {
        databaseContext.SkillTests.Add(skillTest);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return skillTest;
    }

    /// <summary>
    /// Original: PussyCatsApp SkillTestRepository.UpdateSkillTestScore. Targeted column update —
    /// load + mutate + save rather than UPDATE WHERE id; the change-tracker emits exactly the
    /// SET score column SQL.
    /// </summary>
    public async Task UpdateScoreAsync(int skillTestId, int score, CancellationToken cancellationToken = default)
    {
        var test = await databaseContext.SkillTests.FindAsync(new object?[] { skillTestId }, cancellationToken).ConfigureAwait(false);
        if (test is null)
        {
            return;
        }
        test.Score = score;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp SkillTestRepository.UpdateAchievedDate. Same pattern as score:
    /// targeted column update through the change tracker.
    /// </summary>
    public async Task UpdateAchievedDateAsync(int skillTestId, DateOnly achievedDate, CancellationToken cancellationToken = default)
    {
        var test = await databaseContext.SkillTests.FindAsync(new object?[] { skillTestId }, cancellationToken).ConfigureAwait(false);
        if (test is null)
        {
            return;
        }
        test.AchievedDate = achievedDate;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        var test = await databaseContext.SkillTests.FindAsync(new object?[] { skillTestId }, cancellationToken).ConfigureAwait(false);
        if (test is null)
        {
            return;
        }
        databaseContext.SkillTests.Remove(test);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
