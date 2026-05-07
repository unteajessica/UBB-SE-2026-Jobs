using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Recommendations;

public interface IRecommendationRepository
{
    Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Recommendation?> GetLatestByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default);

    Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default);

    Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default);
}
