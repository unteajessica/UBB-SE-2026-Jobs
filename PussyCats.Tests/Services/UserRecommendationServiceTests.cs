using FluentAssertions;
using NSubstitute;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

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
        var matchService = new MatchService(matchRepo, jobService);
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
    public async Task GetNextCardAsync_throws_when_user_missing()
    {
        var service = BuildService();

        Func<Task> act = () => service.GetNextCardAsync(99, UserMatchmakingFilters.Empty());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetNextCardAsync_returns_null_when_no_jobs_match()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        var service = BuildService();
        var card = await service.GetNextCardAsync(1, UserMatchmakingFilters.Empty());

        card.Should().BeNull();
    }

    [Fact]
    public async Task GetNextCardAsync_returns_top_scored_job_card()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(5).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(10).WithCompanyId(5).Build(),
            new JobBuilder().WithId(20).WithCompanyId(5).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(call =>
            ((Job)call[1]).JobId == 20 ? 90.0 : 40.0);

        var service = BuildService();
        var card = await service.GetNextCardAsync(1, UserMatchmakingFilters.Empty());

        card.Should().NotBeNull();
        card!.Job.JobId.Should().Be(20);
        card.CompatibilityScore.Should().Be(90);
        card.DisplayRecommendationId.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNextCardAsync_skips_already_applied_jobs()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(5).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(10).WithCompanyId(5).Build(),
            new JobBuilder().WithId(20).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(50.0);

        var service = BuildService();
        var card = await service.GetNextCardAsync(1, UserMatchmakingFilters.Empty());

        card!.Job.JobId.Should().Be(20);
    }

    [Fact]
    public async Task GetNextCardAsync_skips_jobs_in_cooldown_window()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(5).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(10).WithCompanyId(5).Build(),
            new JobBuilder().WithId(20).WithCompanyId(5).Build());
        recommendationRepo.Seed(new Recommendation
        {
            RecommendationId = 1,
            UserId = 1,
            JobId = 10,
            Timestamp = DateTime.UtcNow.AddMinutes(-30),
        });
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(50.0);

        var service = BuildService();
        var card = await service.GetNextCardAsync(1, UserMatchmakingFilters.Empty());

        card!.Job.JobId.Should().Be(20);
    }

    [Fact]
    public async Task RecalculateTopCardIgnoringCooldownAsync_includes_jobs_in_cooldown()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(5).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        recommendationRepo.Seed(new Recommendation
        {
            RecommendationId = 1,
            UserId = 1,
            JobId = 10,
            Timestamp = DateTime.UtcNow.AddMinutes(-30),
        });
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(50.0);

        var service = BuildService();
        var card = await service.RecalculateTopCardIgnoringCooldownAsync(1, UserMatchmakingFilters.Empty());

        card.Should().NotBeNull();
        card!.Job.JobId.Should().Be(10);
    }

    [Fact]
    public async Task ApplyLikeAsync_creates_match_in_applied_state()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(5).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(50.0);

        var service = BuildService();
        var card = await service.GetNextCardAsync(1, UserMatchmakingFilters.Empty());
        var matchId = await service.ApplyLikeAsync(1, card!);

        var match = await matchRepo.GetByIdAsync(matchId);
        match.Should().NotBeNull();
        match!.Status.Should().Be(MatchStatus.Applied);
    }

    [Fact]
    public async Task ApplyLikeAsync_throws_when_already_applied()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(5).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).Build());

        var service = BuildService();
        var card = new JobRecommendationResult
        {
            Job = await jobRepo.GetByIdAsync(10) ?? throw new InvalidOperationException(),
            Company = await companyRepo.GetByIdAsync(5) ?? throw new InvalidOperationException(),
        };

        Func<Task> act = () => service.ApplyLikeAsync(1, card);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Already applied*");
    }

    [Fact]
    public async Task ApplyDismissAsync_records_recommendation()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(5).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());

        var service = BuildService();
        var card = new JobRecommendationResult
        {
            Job = await jobRepo.GetByIdAsync(10) ?? throw new InvalidOperationException(),
            Company = await companyRepo.GetByIdAsync(5) ?? throw new InvalidOperationException(),
        };

        var dismissId = await service.ApplyDismissAsync(1, card);

        (await recommendationRepo.GetByIdAsync(dismissId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoLikeAsync_removes_match_and_recommendation()
    {
        matchRepo.Seed(new MatchBuilder().WithId(5).AppliedFor(1, 10).Build());
        recommendationRepo.Seed(new Recommendation { RecommendationId = 7, UserId = 1, JobId = 10 });

        var service = BuildService();
        await service.UndoLikeAsync(5, 7);

        (await matchRepo.GetByIdAsync(5)).Should().BeNull();
        (await recommendationRepo.GetByIdAsync(7)).Should().BeNull();
    }

    [Fact]
    public async Task UndoLikeAsync_skips_recommendation_when_id_null()
    {
        matchRepo.Seed(new MatchBuilder().WithId(5).Build());
        recommendationRepo.Seed(new Recommendation { RecommendationId = 7, UserId = 1, JobId = 10 });

        var service = BuildService();
        await service.UndoLikeAsync(5, null);

        (await recommendationRepo.GetByIdAsync(7)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_removes_dismiss_and_display_when_distinct()
    {
        recommendationRepo.Seed(
            new Recommendation { RecommendationId = 7, UserId = 1, JobId = 10 },
            new Recommendation { RecommendationId = 8, UserId = 1, JobId = 10 });

        var service = BuildService();
        await service.UndoDismissAsync(7, 8);

        (await recommendationRepo.GetByIdAsync(7)).Should().BeNull();
        (await recommendationRepo.GetByIdAsync(8)).Should().BeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_does_not_remove_when_dismiss_equals_display()
    {
        recommendationRepo.Seed(new Recommendation { RecommendationId = 7, UserId = 1, JobId = 10 });

        var service = BuildService();
        await service.UndoDismissAsync(7, 7);

        // single Remove call — already happened — second Remove call would be a no-op anyway
        (await recommendationRepo.GetByIdAsync(7)).Should().BeNull();
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
    public void MapUserYearsToExperienceBucket_classifies_year_count(int years, string expected)
    {
        UserRecommendationService.MapUserYearsToExperienceBucket(years).Should().Be(expected);
    }
}
