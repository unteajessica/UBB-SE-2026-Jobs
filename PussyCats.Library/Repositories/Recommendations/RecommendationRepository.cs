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
        databaseContext.Recommendations.Add(recommendation);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return recommendation;
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
