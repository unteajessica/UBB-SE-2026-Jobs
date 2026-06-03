using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Recommendations;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public RecommendationRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <summary>
    /// Tracked single-row lookup. Includes User and Job so callers receive a complete
    /// Recommendation object now that the relationship is represented by navigation properties.
    /// </summary>
    public async Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Recommendations
            .Include(recommendation => recommendation.User)
            .Include(recommendation => recommendation.Job).ThenInclude(job => job.Company)
            .FirstOrDefaultAsync(recommendation => recommendation.RecommendationId == recommendationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Recommendations
            .AsNoTracking()
            .Include(recommendation => recommendation.User)
            .Include(recommendation => recommendation.Job).ThenInclude(job => job.Company)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: matchmaking SqlRecommendationRepository.GetLatestByUserIdAndJobId — preserves
    /// the WHERE+ORDER BY Timestamp DESC TOP(1) semantics by ordering on Timestamp descending
    /// and taking the first row.
    /// </summary>
    public async Task<Recommendation?> GetLatestByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Recommendations
            .AsNoTracking()
            .Include(recommendation => recommendation.User)
            .Include(recommendation => recommendation.Job).ThenInclude(job => job.Company)
            .Where(recommendation =>
                EF.Property<int>(recommendation, "UserId") == userId &&
                EF.Property<int>(recommendation, "JobId") == jobId)
            .OrderByDescending(recommendation => recommendation.Timestamp)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        if (recommendation.Timestamp == default)
        {
            recommendation.Timestamp = DateTime.UtcNow;
        }

        ReconcileExistingNavigation(recommendation);
        databaseContext.Recommendations.Add(recommendation);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return recommendation;
    }

    // DbSet.Add cascades the Added state through navigation properties, so a Recommendation
    // pointing at an existing User/Job would make EF re-INSERT them and trip IDENTITY_INSERT.
    // The incoming User/Job often arrive detached and fully hydrated (e.g. a Job deserialized
    // from an API request still carries Company, RequiredSkills and Matches). Attaching that
    // whole graph crashes when any reachable entity collides with one already tracked. Since a
    // Recommendation only needs the foreign keys, we point each navigation at the already-tracked
    // instance when one exists, otherwise at a key-only stub tracked as Unchanged. Setting state
    // via Entry(...).State is non-recursive, so the incoming object's navigation graph is never
    // walked.
    private void ReconcileExistingNavigation(Recommendation recommendation)
    {
        if (recommendation.User is { UserId: > 0 } incomingUser)
        {
            recommendation.User =
                databaseContext.Users.Local.FirstOrDefault(user => user.UserId == incomingUser.UserId)
                ?? TrackAsUnchanged(new User { UserId = incomingUser.UserId });
        }

        if (recommendation.Job is { JobId: > 0 } incomingJob)
        {
            recommendation.Job =
                databaseContext.Jobs.Local.FirstOrDefault(job => job.JobId == incomingJob.JobId)
                ?? TrackAsUnchanged(new Job { JobId = incomingJob.JobId });
        }
    }

    // Tracks ONLY this stub (no navigation graph) as a preexisting row, so the subsequent
    // Recommendation insert reads its key as the foreign key without touching the real row.
    private TEntity TrackAsUnchanged<TEntity>(TEntity stub)
        where TEntity : class
    {
        databaseContext.Entry(stub).State = EntityState.Unchanged;
        return stub;
    }

    public async Task UpdateAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        databaseContext.Recommendations.Update(recommendation);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        var recommendation = await databaseContext.Recommendations.FindAsync(new object?[] { recommendationId }, cancellationToken).ConfigureAwait(false);
        if (recommendation is null)
        {
            return;
        }
        databaseContext.Recommendations.Remove(recommendation);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
