using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;
using PussyCats_App.Services.CooldownService;

namespace PussyCats.Tests.Services;

public class CooldownServiceTests
{
    private readonly FakeRecommendationRepository recommendationRepository = new();
    private readonly CooldownService service;
    private readonly TimeSpan cooldown = TimeSpan.FromHours(24);

    public CooldownServiceTests()
    {
        service = new CooldownService(recommendationRepository, cooldown);
    }

    [Fact]
    public async Task IsOnCooldownAsync_NoRecommendationExists_ReturnsFalse()
    {
        const int userId = 1, jobId = 10;
        (await service.IsOnCooldownAsync(userId, jobId, DateTime.UtcNow)).Should().BeFalse();
    }

    [Fact]
    public async Task IsOnCooldownAsync_RecommendationWithinCooldownPeriod_ReturnsTrue()
    {
        var currentDate = DateTime.UtcNow;
        const int minutesAgo = 30;
        const int userId = 1, jobId = 10;
        recommendationRepository.Seed(new Recommendation
        {
            RecommendationId = 1,
            User = new User { UserId = userId },
            Job = new Job { JobId = jobId },
            Timestamp = currentDate.AddMinutes(-minutesAgo),
        });

        (await service.IsOnCooldownAsync(userId, jobId, currentDate)).Should().BeTrue();
    }

    [Fact]
    public async Task IsOnCooldownAsync_RecommendationOlderThanCooldown_ReturnsFalse()
    {
        var currentDate = DateTime.UtcNow;
        const int daysAgo = 2;
        const int userId = 1, jobId = 10;
        recommendationRepository.Seed(new Recommendation
        {
            RecommendationId = 1,
            User = new User { UserId = userId },
            Job = new Job { JobId = jobId },
            Timestamp = currentDate.AddDays(-daysAgo),
        });

        (await service.IsOnCooldownAsync(userId, jobId, currentDate)).Should().BeFalse();
    }

    [Fact]
    public async Task IsOnCooldownAsync_MultipleRecommendationsExist_UsesLatestTimestamp()
    {
        var currentDate = DateTime.UtcNow;
        const int userId = 1, jobId = 10;
        const int daysAgo = 7, minutesAgo = 10;
        recommendationRepository.Seed(
            new Recommendation { RecommendationId = 1, User = new User { UserId = userId }, Job = new Job { JobId = jobId }, Timestamp = currentDate.AddDays(-daysAgo) },
            new Recommendation { RecommendationId = 2, User = new User { UserId = userId }, Job = new Job { JobId = jobId }, Timestamp = currentDate.AddMinutes(-minutesAgo) });

        (await service.IsOnCooldownAsync(userId, jobId, currentDate)).Should().BeTrue();
    }

    [Fact]
    public async Task Cooldown_ZeroOrNegativeTimeSpanProvided_FallsBackToDefault24Hours()
    {
        var zeroService = new CooldownService(recommendationRepository, TimeSpan.Zero);
        var negativeService = new CooldownService(recommendationRepository, TimeSpan.FromHours(-1));

        const int userId = 1, jobId = 10, hoursAgo = 12;
        var now = DateTime.UtcNow;
        recommendationRepository.Seed(new Recommendation { RecommendationId = 1, User = new User { UserId = userId }, Job = new Job { JobId = jobId }, Timestamp = now.AddHours(-hoursAgo) });

        (await negativeService.IsOnCooldownAsync(userId, jobId, now)).Should().BeTrue();
        (await zeroService.IsOnCooldownAsync(userId, jobId, now)).Should().BeTrue();
    }
}
