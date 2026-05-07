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
    /// Tracked single-row lookup. No User/Job include — callers that need them ask for the User
    /// or Job repository directly.
    /// </summary>
    public async Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Recommendations
            .FirstOrDefaultAsync(recommendation => recommendation.RecommendationId == recommendationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Recommendations
            .AsNoTracking()
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
            .Where(recommendation => recommendation.UserId == userId && recommendation.JobId == jobId)
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
