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
    public async Task IsOnCooldownAsync_returns_false_when_no_recommendation_exists()
    {
        (await service.IsOnCooldownAsync(1, 10, DateTime.UtcNow)).Should().BeFalse();
    }

    [Fact]
    public async Task IsOnCooldownAsync_returns_true_when_recent_recommendation_within_cooldown()
    {
        var now = DateTime.UtcNow;
        repo.Seed(new Recommendation
        {
            RecommendationId = 1,
            UserId = 1,
            JobId = 10,
            Timestamp = now.AddMinutes(-30),
        });

        (await service.IsOnCooldownAsync(1, 10, now)).Should().BeTrue();
    }

    [Fact]
    public async Task IsOnCooldownAsync_returns_false_when_recommendation_older_than_cooldown()
    {
        var now = DateTime.UtcNow;
        repo.Seed(new Recommendation
        {
            RecommendationId = 1,
            UserId = 1,
            JobId = 10,
            Timestamp = now.AddDays(-2),
        });

        (await service.IsOnCooldownAsync(1, 10, now)).Should().BeFalse();
    }

    [Fact]
    public async Task IsOnCooldownAsync_uses_latest_when_multiple_exist()
    {
        var now = DateTime.UtcNow;
        repo.Seed(
            new Recommendation { RecommendationId = 1, UserId = 1, JobId = 10, Timestamp = now.AddDays(-7) },
            new Recommendation { RecommendationId = 2, UserId = 1, JobId = 10, Timestamp = now.AddMinutes(-10) });

        (await service.IsOnCooldownAsync(1, 10, now)).Should().BeTrue();
    }

    [Fact]
    public async Task Cooldown_falls_back_to_24h_when_zero_or_negative_provided()
    {
        var zeroService = new CooldownService(repo, TimeSpan.Zero);
        var negativeService = new CooldownService(repo, TimeSpan.FromHours(-1));

        // No public accessor for the period; smoke-test by adding a recommendation
        // 12h ago and confirming both fall-back instances treat it as on-cooldown.
        var now = DateTime.UtcNow;
        repo.Seed(new Recommendation { RecommendationId = 1, UserId = 1, JobId = 10, Timestamp = now.AddHours(-12) });

        (await zeroService.IsOnCooldownAsync(1, 10, now)).Should().BeTrue();
        (await negativeService.IsOnCooldownAsync(1, 10, now)).Should().BeTrue();
    }
}
