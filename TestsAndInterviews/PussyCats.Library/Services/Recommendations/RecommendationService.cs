using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Recommendations;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.Library.Services.Recommendations;

public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationRepository recommendationRepository;
    private readonly IUserRepository userRepository;
    private readonly IJobRepository jobRepository;

    public RecommendationService(
        IRecommendationRepository recommendationRepository,
        IUserRepository userRepository,
        IJobRepository jobRepository)
    {
        this.recommendationRepository = recommendationRepository;
        this.userRepository = userRepository;
        this.jobRepository = jobRepository;
    }

    public async Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await recommendationRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        return await recommendationRepository.GetByIdAsync(recommendationId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Recommendation?> GetLatestForUserAndJobAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        return await recommendationRepository.GetLatestByUserIdAndJobIdAsync(userId, jobId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Recommendation> AddAsync(int userId, int jobId, DateTime? timestamp, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Job {jobId} not found.");

        var recommendation = new Recommendation
        {
            User = user,
            Job = job,
            Timestamp = timestamp ?? DateTime.UtcNow,
        };

        return await recommendationRepository.AddAsync(recommendation, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateTimestampAsync(int recommendationId, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        var recommendation = await recommendationRepository.GetByIdAsync(recommendationId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Recommendation {recommendationId} not found.");

        recommendation.Timestamp = timestamp;
        await recommendationRepository.UpdateAsync(recommendation, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        await recommendationRepository.RemoveAsync(recommendationId, cancellationToken).ConfigureAwait(false);
    }
}
