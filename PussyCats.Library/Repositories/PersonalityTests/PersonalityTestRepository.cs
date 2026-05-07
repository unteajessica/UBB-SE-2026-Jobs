using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.PersonalityTests;

public class PersonalityTestRepository : IPersonalityTestRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public PersonalityTestRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <summary>
    /// Includes TraitScores so a single query returns the complete result. Tracked because
    /// PersonalityTestService updates trait scores in place when the user re-takes the test.
    /// Note: this replaces PussyCatsApp's PersonalityTestRepository.Load, which previously read
    /// a single string column off the Users table and returned that. The new model stores trait
    /// scores as first-class rows, so the return shape is the structured PersonalityTestResult.
    /// </summary>
    public async Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.PersonalityTestResults
            .Include(result => result.TraitScores)
            .FirstOrDefaultAsync(result => result.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<PersonalityTestResult> AddAsync(PersonalityTestResult result, CancellationToken cancellationToken = default)
    {
        if (result.CompletedAt == default)
        {
            result.CompletedAt = DateTime.UtcNow;
        }
        databaseContext.PersonalityTestResults.Add(result);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task UpdateAsync(PersonalityTestResult result, CancellationToken cancellationToken = default)
    {
        var tracked = databaseContext.PersonalityTestResults.Local.FirstOrDefault(existing => existing.PersonalityTestResultId == result.PersonalityTestResultId);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(result);
        }
        else
        {
            databaseContext.Entry(result).State = EntityState.Modified;
        }
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int personalityTestResultId, CancellationToken cancellationToken = default)
    {
        var result = await databaseContext.PersonalityTestResults.FindAsync(new object?[] { personalityTestResultId }, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return;
        }
        databaseContext.PersonalityTestResults.Remove(result);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
