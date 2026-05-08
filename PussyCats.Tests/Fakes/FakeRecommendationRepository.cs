using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Recommendations;

namespace PussyCats.Tests.Fakes;

public class FakeRecommendationRepository : IRecommendationRepository
{
    private readonly Dictionary<int, Recommendation> store = new();

    public void Seed(params Recommendation[] recommendations)
    {
        foreach (var recommendation in recommendations)
        {
            store[recommendation.RecommendationId] = recommendation;
        }
    }

    public Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        store.TryGetValue(recommendationId, out var recommendation);
        return Task.FromResult(recommendation);
    }

    public Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Recommendation> snapshot = store.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<Recommendation?> GetLatestByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        var latest = store.Values
            .Where(recommendation => recommendation.UserId == userId && recommendation.JobId == jobId)
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
        store[recommendation.RecommendationId] = recommendation;
        return Task.FromResult(recommendation);
    }

    public Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        store.Remove(recommendationId);
        return Task.CompletedTask;
    }

    private int NextId() => store.Count == 0 ? 1 : store.Keys.Max() + 1;
}
