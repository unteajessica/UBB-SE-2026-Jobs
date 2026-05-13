using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Services;

public class CooldownServiceTests
{
    private readonly FakeRecommendationRepository repo = new();
    private readonly CooldownService service;
    private readonly TimeSpan cooldown = TimeSpan.FromHours(24);

    public CooldownServiceTests()
    {
        service = new CooldownService(repo, cooldown);
    }

    [Fact]
    public async Task IsOnCooldownAsync_NoRecommendationExists_ReturnsFalse()
    {
        (await service.IsOnCooldownAsync(1, 10, DateTime.UtcNow)).Should().BeFalse();
    }

    [Fact]
    public async Task IsOnCooldownAsync_RecommendationWithinCooldownPeriod_ReturnsTrue()
    {
        var currentDate = DateTime.UtcNow;
        repo.Seed(new Recommendation
        {
            RecommendationId = 1,
            User = new User { UserId = 1 },
            Job = new Job { JobId = 10 },
            Timestamp = currentDate.AddMinutes(-30),
        });

        (await service.IsOnCooldownAsync(1, 10, currentDate)).Should().BeTrue();
    }

    [Fact]
    public async Task IsOnCooldownAsync_RecommendationOlderThanCooldown_ReturnsFalse()
    {
        var currentDate = DateTime.UtcNow;
        repo.Seed(new Recommendation
        {
            RecommendationId = 1,
            User = new User { UserId = 1 },
            Job = new Job { JobId = 10 },
            Timestamp = currentDate.AddDays(-2),
        });

        (await service.IsOnCooldownAsync(1, 10, currentDate)).Should().BeFalse();
    }

    [Fact]
    public async Task IsOnCooldownAsync_MultipleRecommendationsExist_UsesLatestTimestamp()
    {
        var currentDate = DateTime.UtcNow;
        repo.Seed(
            new Recommendation { RecommendationId = 1, User = new User { UserId = 1 }, Job = new Job { JobId = 10 }, Timestamp = currentDate.AddDays(-7) },
            new Recommendation { RecommendationId = 2, User = new User { UserId = 1 }, Job = new Job { JobId = 10 }, Timestamp = currentDate.AddMinutes(-10) });

        (await service.IsOnCooldownAsync(1, 10, currentDate)).Should().BeTrue();
    }

    [Fact]
    public async Task Cooldown_ZeroOrNegativeTimeSpanProvided_FallsBackToDefault24Hours()
    {
        var zeroService = new CooldownService(repo, TimeSpan.Zero);
        var negativeService = new CooldownService(repo, TimeSpan.FromHours(-1));

        var now = DateTime.UtcNow;
        repo.Seed(new Recommendation { RecommendationId = 1, User = new User { UserId = 1 }, Job = new Job { JobId = 10 }, Timestamp = now.AddHours(-12) });

        (await negativeService.IsOnCooldownAsync(1, 10, now)).Should().BeTrue();
        (await zeroService.IsOnCooldownAsync(1, 10, now)).Should().BeTrue();
    }
}
