using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.Recommendations;

public interface IRecommendationService
{
    Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default);

    Task<Recommendation?> GetLatestForUserAndJobAsync(int userId, int jobId, CancellationToken cancellationToken = default);

    Task<Recommendation> AddAsync(int userId, int jobId, DateTime? timestamp, CancellationToken cancellationToken = default);

    Task UpdateTimestampAsync(int recommendationId, DateTime timestamp, CancellationToken cancellationToken = default);

    Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default);
}
