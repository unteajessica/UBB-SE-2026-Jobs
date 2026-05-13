using FluentAssertions;
using NSubstitute;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using System.Security.Cryptography;

namespace PussyCats.Tests.Services;

public class UserRecommendationServiceTests
{
    private readonly FakeUserRepository userRepo = new();
    private readonly FakeJobRepository jobRepo = new();
    private readonly FakeUserSkillRepository userSkillRepo = new();
    private readonly FakeJobSkillRepository jobSkillRepo = new();
    private readonly FakeCompanyRepository companyRepo = new();
    private readonly FakeMatchRepository matchRepo = new();
    private readonly FakeRecommendationRepository recommendationRepo = new();
    private readonly IRecommendationAlgorithm algorithm = Substitute.For<IRecommendationAlgorithm>();

    private UserRecommendationService BuildService(TimeSpan? cooldownPeriod = null)
    {
        var jobService = new JobService(jobRepo);
        var matchService = new MatchService(matchRepo, jobService, new UserService(userRepo));
        var cooldown = new CooldownService(recommendationRepo, cooldownPeriod ?? TimeSpan.FromHours(24));
        return new UserRecommendationService(
            userRepo,
            jobRepo,
            userSkillRepo,
            jobSkillRepo,
            companyRepo,
            matchService,
            recommendationRepo,
            cooldown,
            algorithm);
    }

    [Fact]
    public async Task GetNextCardAsync_UserIsMissing_ThrowsInvalidOperationException()
    {
        const int nonExistentUserId = 999;
        var service = BuildService();

        Func<Task> act = () => service.GetNextCardAsync(nonExistentUserId, UserMatchmakingFilters.Empty());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetNextCardAsync_NoJobsMatchFilters_ReturnsNull()
    {
        const int userId = 1;
        userRepo.Seed(new UserBuilder().WithId(userId).Build());

        var service = BuildService();
        var card = await service.GetNextCardAsync(1, UserMatchmakingFilters.Empty());

        card.Should().BeNull();
    }

    [Fact]
    public async Task GetNextCardAsync_MultipleJobsAvailable_ReturnsTopScoredJobCard()
    {
        const int userId = 1;
        const int topJobId = 20;
        const int lowJobId = 10;
        const double topScore = 99.0;
        const int companyId = 5;

        userRepo.Seed(new UserBuilder().WithId(userId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(lowJobId).WithCompanyId(companyId).Build(),
            new JobBuilder().WithId(topJobId).WithCompanyId(companyId).Build());

        algorithm.CalculateCompatibilityScore(Arg.Any<User>(), Arg.Is<Job>(job => job.JobId == topJobId), Arg.Any<IReadOnlyList<UserSkill>>(), Arg.Any<IReadOnlyList<JobSkill>>())
            .Returns(topScore);

        var service = BuildService();
        var card = await service.GetNextCardAsync(userId, UserMatchmakingFilters.Empty());

        card!.Job.JobId.Should().Be(topJobId);
    }

    [Fact]
    public async Task GetNextCardAsync_JobInCooldown_SkipsToNextAvailableJob()
    {
        const int userId = 1;
        const int recentJobId = 10;
        const int availableJobId = 20;
        const int companyId = 5;
        const double expectedCompatibilityScore = 50.0;
        UserMatchmakingFilters filters = UserMatchmakingFilters.Empty();

        userRepo.Seed(new UserBuilder().WithId(userId).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(recentJobId).WithCompanyId(companyId).Build(),
            new JobBuilder().WithId(availableJobId).WithCompanyId(companyId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(companyId).Build());

        recommendationRepo.Seed(new Recommendation { UserId = userId, JobId = recentJobId, Timestamp = DateTime.UtcNow });
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(expectedCompatibilityScore);

        var card = await BuildService().GetNextCardAsync(userId, UserMatchmakingFilters.Empty());

        card!.Job.JobId.Should().Be(availableJobId);
    }

    
    [Fact]
    public async Task RecalculateTopCardIgnoringCooldownAsync_JobInCooldown_IncludesJobsInCooldown()
    {
        int userId = 1;
        int jobId = 10;
        int companyId = 5;
        int recommendationId = 100;
        double defaultScore = 50.0;
        UserMatchmakingFilters filters = UserMatchmakingFilters.Empty();

        userRepo.Seed(new UserBuilder().WithId(userId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        recommendationRepo.Seed(new Recommendation
        {
            RecommendationId = recommendationId,
            UserId = userId,
            JobId = jobId,
            Timestamp = DateTime.UtcNow.AddMinutes(-30),
        });
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(defaultScore);

        UserRecommendationService service = BuildService();
        JobRecommendationResult? card = await service.RecalculateTopCardIgnoringCooldownAsync(userId, filters);

        card!.Job.JobId.Should().Be(jobId);
    }

    [Fact]
    public async Task ApplyLikeAsync_ValidCardProvided_CreatesMatchInAppliedState()
    {
        int userId = 1;
        int jobId = 10;
        int companyId = 5;
        double defaultScore = 50.0;
        UserMatchmakingFilters filters = UserMatchmakingFilters.Empty();

        userRepo.Seed(new UserBuilder().WithId(userId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(defaultScore);

        UserRecommendationService service = BuildService();
        JobRecommendationResult? card = await service.GetNextCardAsync(userId, filters);
        int matchId = await service.ApplyLikeAsync(userId, card!);

        Match? match = await matchRepo.GetByIdAsync(matchId);
        match.Should().NotBeNull();
        match!.Status.Should().Be(MatchStatus.Applied);
    }

    [Fact]
    public async Task ApplyLikeAsync_MatchExists_ThrowsInvalidOperationException()
    {
        int userId = 1;
        int jobId = 10;
        int companyId = 5;
        int matchId = 500;

        userRepo.Seed(new UserBuilder().WithId(userId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepo.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, jobId).Build());

        UserRecommendationService service = BuildService();
        JobRecommendationResult card = new JobRecommendationResult
        {
            Job = await jobRepo.GetByIdAsync(jobId) ?? throw new InvalidOperationException(),
            Company = await companyRepo.GetByIdAsync(companyId) ?? throw new InvalidOperationException(),
        };

        Func<Task> act = () => service.ApplyLikeAsync(userId, card);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Already applied*");
    }

    [Fact]
    public async Task ApplyDismissAsync_ValidCardProvided_RecordsRecommendation()
    {
        const int userId = 1;
        const int jobId = 10;
        const int companyId = 5;

        userRepo.Seed(new UserBuilder().WithId(userId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());

        var service = BuildService();
        var card = new JobRecommendationResult
        {
            Job = await jobRepo.GetByIdAsync(jobId) ?? throw new InvalidOperationException(),
            Company = await companyRepo.GetByIdAsync(companyId) ?? throw new InvalidOperationException(),
        };

        var dismissId = await service.ApplyDismissAsync(1, card);

        (await recommendationRepo.GetByIdAsync(dismissId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoLikeAsync_IdsProvided_RemovesMatchAndRecommendation()
    {
        int matchId = 5;
        int recommendationId = 7;
        int userId = 1;
        int jobId = 10;

        matchRepo.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, jobId).Build());
        recommendationRepo.Seed(new Recommendation { RecommendationId = recommendationId, UserId = userId, JobId = jobId });

        UserRecommendationService service = BuildService();
        await service.UndoLikeAsync(matchId, recommendationId);

        (await matchRepo.GetByIdAsync(matchId)).Should().BeNull();
        (await recommendationRepo.GetByIdAsync(recommendationId)).Should().BeNull();
    }
    

    [Fact]
    public async Task UndoLikeAsync_RecommendationIdIsNull_SkipsRecommendationRemoval()
    {
        matchRepo.Seed(new MatchBuilder().WithId(5).Build());
        recommendationRepo.Seed(new Recommendation { RecommendationId = 7, UserId = 1, JobId = 10 });

        var service = BuildService();
        await service.UndoLikeAsync(5, null);

        (await recommendationRepo.GetByIdAsync(7)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_DistinctIdsProvided_RemovesDismissAndDisplayRecommendations()
    {
        int dismissId = 7;
        int displayId = 8;
        int userId = 1;
        int jobId = 10;

        recommendationRepo.Seed(
            new Recommendation { RecommendationId = dismissId, UserId = userId, JobId = jobId },
            new Recommendation { RecommendationId = displayId, UserId = userId, JobId = jobId });

        UserRecommendationService service = BuildService();
        await service.UndoDismissAsync(dismissId, displayId);

        (await recommendationRepo.GetByIdAsync(dismissId)).Should().BeNull();
        (await recommendationRepo.GetByIdAsync(displayId)).Should().BeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_IdenticalIdsProvided_RemovesSingleRecommendation()
    {
        int recommendationId = 7;
        int userId = 1;
        int jobId = 10;

        recommendationRepo.Seed(new Recommendation { RecommendationId = recommendationId, UserId = userId, JobId = jobId });

        UserRecommendationService service = BuildService();
        await service.UndoDismissAsync(recommendationId, recommendationId);

        (await recommendationRepo.GetByIdAsync(recommendationId)).Should().BeNull();
    }

    [Theory]
    [InlineData(0, "Internship")]
    [InlineData(1, "Internship")]
    [InlineData(2, "Entry")]
    [InlineData(3, "Entry")]
    [InlineData(4, "MidSenior")]
    [InlineData(6, "MidSenior")]
    [InlineData(7, "Director")]
    [InlineData(9, "Director")]
    [InlineData(10, "Executive")]
    [InlineData(25, "Executive")]
    public void MapUserYearsToExperienceBucket_YearsProvided_ClassifiesYearCountIntoCorrectBucket(int years, string expected)
    {
        UserRecommendationService.MapUserYearsToExperienceBucket(years).Should().Be(expected);
    }
}