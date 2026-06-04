using PussyCats.Library.Domain;
using PussyCats.Library.Services.Recommendations;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class RecommendationServiceTests
{
    private readonly FakeRecommendationRepository recommendationRepository = new();
    private readonly FakeUserRepository userRepository = new();
    private readonly FakeJobRepository jobRepository = new();
    private readonly RecommendationService service;

    public RecommendationServiceTests()
    {
        service = new RecommendationService(recommendationRepository, userRepository, jobRepository);
    }

    [Fact]
    public async Task AddAsync_UserAndJobExist_PersistsRecommendation()
    {
        var user = new UserBuilder().WithId(1).Build();
        var job = new JobBuilder().WithId(2).Build();
        userRepository.Seed(user);
        jobRepository.Seed(job);

        var saved = await service.AddAsync(user.UserId, job.JobId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(user.UserId, saved.User.UserId);
        Assert.Equal(job.JobId, saved.Job.JobId);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), saved.Timestamp);
        Assert.Equal(1, (await recommendationRepository.GetAllAsync()).Count());
    }

    [Fact]
    public async Task AddAsync_NullTimestamp_DefaultsToUtcNow()
    {
        var user = new UserBuilder().WithId(1).Build();
        var job = new JobBuilder().WithId(2).Build();
        userRepository.Seed(user);
        jobRepository.Seed(job);

        var before = DateTime.UtcNow;
        var saved = await service.AddAsync(user.UserId, job.JobId, null);
        var after = DateTime.UtcNow;

        Assert.True(saved.Timestamp >= before);
        Assert.True(saved.Timestamp <= after);
    }

    [Fact]
    public async Task AddAsync_UserMissing_ThrowsKeyNotFound()
    {
        jobRepository.Seed(new JobBuilder().WithId(2).Build());

        Func<Task> act = () => service.AddAsync(999, 2, DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public async Task AddAsync_JobMissing_ThrowsKeyNotFound()
    {
        userRepository.Seed(new UserBuilder().WithId(1).Build());

        Func<Task> act = () => service.AddAsync(1, 999, DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public async Task UpdateTimestampAsync_RecommendationExists_PersistsNewTimestamp()
    {
        var user = new UserBuilder().WithId(1).Build();
        var job = new JobBuilder().WithId(2).Build();
        var recommendation = new Recommendation
        {
            RecommendationId = 7,
            User = user,
            Job = job,
            Timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        recommendationRepository.Seed(recommendation);

        await service.UpdateTimestampAsync(7, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var refreshed = await recommendationRepository.GetByIdAsync(7);
        Assert.Equal(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), refreshed!.Timestamp);
    }

    [Fact]
    public async Task UpdateTimestampAsync_RecommendationMissing_ThrowsKeyNotFound()
    {
        Func<Task> act = () => service.UpdateTimestampAsync(404, DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains("404", ex.Message);
    }

    [Fact]
    public async Task RemoveAsync_DelegatesToRepository()
    {
        var user = new UserBuilder().WithId(1).Build();
        var job = new JobBuilder().WithId(2).Build();
        recommendationRepository.Seed(new Recommendation { RecommendationId = 5, User = user, Job = job });

        await service.RemoveAsync(5);

        Assert.Null(await recommendationRepository.GetByIdAsync(5));
    }

    [Fact]
    public async Task GetLatestForUserAndJobAsync_DelegatesToRepository()
    {
        var user = new UserBuilder().WithId(1).Build();
        var job = new JobBuilder().WithId(2).Build();
        recommendationRepository.Seed(
            new Recommendation { RecommendationId = 1, User = user, Job = job, Timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Recommendation { RecommendationId = 2, User = user, Job = job, Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        var latest = await service.GetLatestForUserAndJobAsync(user.UserId, job.JobId);

        Assert.Equal(2, latest!.RecommendationId);
    }
}
