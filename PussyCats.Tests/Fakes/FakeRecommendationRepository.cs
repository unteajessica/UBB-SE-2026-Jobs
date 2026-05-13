using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Recommendations;

namespace PussyCats.Tests.Fakes;

public class FakeRecommendationRepository : IRecommendationRepository
{
    private readonly Dictionary<int, Recommendation> recommendationsById = new();

    public void Seed(params Recommendation[] recommendations)
    {
        foreach (var recommendation in recommendations)
        {
            recommendationsById[recommendation.RecommendationId] = recommendation;
        }
    }

    public Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        recommendationsById.TryGetValue(recommendationId, out var recommendation);
        return Task.FromResult(recommendation);
    }

    public Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Recommendation> snapshot = recommendationsById.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<Recommendation?> GetLatestByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        var latest = recommendationsById.Values
            .Where(recommendation => recommendation.User.UserId == userId && recommendation.Job.JobId == jobId)
            .OrderByDescending(recommendation => recommendation.Timestamp)
            .FirstOrDefault();
        return Task.FromResult(latest);
    }

    public Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        if (recommendation.RecommendationId == 0)
        {
            recommendation.RecommendationId = NextId();
        }
        if (recommendation.Timestamp == default)
        {
            recommendation.Timestamp = DateTime.UtcNow;
        }
        recommendationsById[recommendation.RecommendationId] = recommendation;
        return Task.FromResult(recommendation);
    }

    public Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        recommendationsById.Remove(recommendationId);
        return Task.CompletedTask;
    }

    private int NextId() => recommendationsById.Count == 0 ? 1 : recommendationsById.Keys.Max() + 1;
}
