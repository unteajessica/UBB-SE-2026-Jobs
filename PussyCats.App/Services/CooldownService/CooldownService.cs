using PussyCats.Library.Repositories.Recommendations;

namespace PussyCats_App.Services.CooldownService;

public sealed class CooldownService : ICooldownService
{
    private readonly IRecommendationRepository recommendationRepository;
    private readonly TimeSpan cooldownPeriod;

    public CooldownService(IRecommendationRepository recommendationRepository, TimeSpan cooldownPeriod)
    {
        this.recommendationRepository = recommendationRepository;
        this.cooldownPeriod = cooldownPeriod <= TimeSpan.Zero ? TimeSpan.FromHours(24) : cooldownPeriod;
    }

    public async Task<bool> IsOnCooldownAsync(int userId, int jobId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var latest = await recommendationRepository.GetLatestByUserIdAndJobIdAsync(userId, jobId, cancellationToken).ConfigureAwait(false);
        if (latest is null)
        {
            return false;
        }

        var elapsed = utcNow - NormalizeToUtc(latest.Timestamp);
        return elapsed < cooldownPeriod;
    }

    private static DateTime NormalizeToUtc(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Local).ToUniversalTime()
        };
    }
}
