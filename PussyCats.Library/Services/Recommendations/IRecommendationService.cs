using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.Recommendations;

public interface IRecommendationService
{
    Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default);

    Task<Recommendation?> GetLatestForUserAndJobAsync(int userId, int jobId, CancellationToken cancellationToken = default);

    // Resolves User and Job by id, builds the Recommendation, persists it. Throws
    // KeyNotFoundException if either lookup fails — controllers should map to 404.
    Task<Recommendation> AddAsync(int userId, int jobId, DateTime? timestamp, CancellationToken cancellationToken = default);

    // Updates the timestamp of an existing recommendation. Throws KeyNotFoundException
    // if the recommendation doesn't exist.
    Task UpdateTimestampAsync(int recommendationId, DateTime timestamp, CancellationToken cancellationToken = default);

    Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default);
}
