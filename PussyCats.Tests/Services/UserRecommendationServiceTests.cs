using FluentAssertions;
using NSubstitute;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class UserRecommendationServiceTests
{
    private const int MissingUserId = 99;
    private const int ExistingUserId = 1;
    private const int CompanyId = 5;
    private const int PrimaryJobId = 10;
    private const int SecondaryJobId = 20;
    private const int MatchId = 1;
    private const int AlternateMatchId = 5;
    private const int RecommendationId = 1;
    private const int UndoRecommendationId = 7;
    private const int AlternateRecommendationId = 8;
    private const int CooldownHours = 24;
    private const int RecentMinutes = 30;

    private const double TopScore = 90.0;
    private const double SecondaryScore = 40.0;
    private const double DefaultScore = 50.0;

    private const int YearsInternshipFloor = 0;
    private const int YearsInternshipUpper = 1;
    private const int YearsEntryFloor = 2;
    private const int YearsEntryUpper = 3;
    private const int YearsMidSeniorFloor = 4;
    private const int YearsMidSeniorUpper = 6;
    private const int YearsDirectorFloor = 7;
    private const int YearsDirectorUpper = 9;
    private const int YearsExecutiveFloor = 10;
    private const int YearsExecutiveUpper = 25;

    public static IEnumerable<object[]> ExperienceBucketCases =>
    [
        new object[] { YearsInternshipFloor, "Internship" },
        new object[] { YearsInternshipUpper, "Internship" },
        new object[] { YearsEntryFloor, "Entry" },
        new object[] { YearsEntryUpper, "Entry" },
        new object[] { YearsMidSeniorFloor, "MidSenior" },
        new object[] { YearsMidSeniorUpper, "MidSenior" },
        new object[] { YearsDirectorFloor, "Director" },
        new object[] { YearsDirectorUpper, "Director" },
        new object[] { YearsExecutiveFloor, "Executive" },
        new object[] { YearsExecutiveUpper, "Executive" },
    ];

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
        var cooldown = new CooldownService(recommendationRepo, cooldownPeriod ?? TimeSpan.FromHours(CooldownHours));
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
        var service = BuildService();

        Func<Task> act = () => service.GetNextCardAsync(MissingUserId, UserMatchmakingFilters.Empty());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetNextCardAsync_NoJobsMatchFilters_ReturnsNull()
    {
        userRepo.Seed(new UserBuilder().WithId(ExistingUserId).Build());

        var service = BuildService();
        var card = await service.GetNextCardAsync(ExistingUserId, UserMatchmakingFilters.Empty());

        card.Should().BeNull();
    }

    [Fact]
    public async Task GetNextCardAsync_MultipleJobsAvailable_ReturnsTopScoredJobCard()
    {
        userRepo.Seed(new UserBuilder().WithId(ExistingUserId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(PrimaryJobId).WithCompanyId(CompanyId).Build(),
            new JobBuilder().WithId(SecondaryJobId).WithCompanyId(CompanyId).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(call =>
            ((Job)call[1]).JobId == SecondaryJobId ? TopScore : SecondaryScore);

        var service = BuildService();
        var card = await service.GetNextCardAsync(ExistingUserId, UserMatchmakingFilters.Empty());

        card.Should().NotBeNull();
        card!.Job.JobId.Should().Be(SecondaryJobId);
        card.CompatibilityScore.Should().Be(TopScore);
        card.DisplayRecommendationId.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNextCardAsync_UserAlreadyApplied_SkipsAlreadyAppliedJobs()
    {
        userRepo.Seed(new UserBuilder().WithId(ExistingUserId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(PrimaryJobId).WithCompanyId(CompanyId).Build(),
            new JobBuilder().WithId(SecondaryJobId).WithCompanyId(CompanyId).Build());
        matchRepo.Seed(new MatchBuilder().WithId(MatchId).AppliedFor(ExistingUserId, PrimaryJobId).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(DefaultScore);

        var service = BuildService();
        var card = await service.GetNextCardAsync(ExistingUserId, UserMatchmakingFilters.Empty());

        card!.Job.JobId.Should().Be(SecondaryJobId);
    }

    [Fact]
    public async Task GetNextCardAsync_JobRecentlySeen_SkipsJobsInCooldownWindow()
    {
        userRepo.Seed(new UserBuilder().WithId(ExistingUserId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(PrimaryJobId).WithCompanyId(CompanyId).Build(),
            new JobBuilder().WithId(SecondaryJobId).WithCompanyId(CompanyId).Build());
        recommendationRepo.Seed(new Recommendation
        {
            RecommendationId = RecommendationId,
            UserId = ExistingUserId,
            JobId = PrimaryJobId,
            Timestamp = DateTime.UtcNow.AddMinutes(-RecentMinutes),
        });
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(DefaultScore);

        var service = BuildService();
        var card = await service.GetNextCardAsync(ExistingUserId, UserMatchmakingFilters.Empty());

        card!.Job.JobId.Should().Be(SecondaryJobId);
    }

    [Fact]
    public async Task RecalculateTopCardIgnoringCooldownAsync_JobInCooldown_IncludesJobsInCooldown()
    {
        userRepo.Seed(new UserBuilder().WithId(ExistingUserId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(PrimaryJobId).WithCompanyId(CompanyId).Build());
        recommendationRepo.Seed(new Recommendation
        {
            RecommendationId = RecommendationId,
            UserId = ExistingUserId,
            JobId = PrimaryJobId,
            Timestamp = DateTime.UtcNow.AddMinutes(-RecentMinutes),
        });
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(DefaultScore);

        var service = BuildService();
        var card = await service.RecalculateTopCardIgnoringCooldownAsync(ExistingUserId, UserMatchmakingFilters.Empty());

        card.Should().NotBeNull();
        card!.Job.JobId.Should().Be(PrimaryJobId);
    }

    [Fact]
    public async Task ApplyLikeAsync_ValidCardProvided_CreatesMatchInAppliedState()
    {
        userRepo.Seed(new UserBuilder().WithId(ExistingUserId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(PrimaryJobId).WithCompanyId(CompanyId).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(DefaultScore);

        var service = BuildService();
        var card = await service.GetNextCardAsync(ExistingUserId, UserMatchmakingFilters.Empty());
        var matchId = await service.ApplyLikeAsync(ExistingUserId, card!);

        var match = await matchRepo.GetByIdAsync(matchId);
        match.Should().NotBeNull();
        match!.Status.Should().Be(MatchStatus.Applied);
    }

    [Fact]
    public async Task ApplyLikeAsync_MatchExists_ThrowsInvalidOperationException()
    {
        userRepo.Seed(new UserBuilder().WithId(ExistingUserId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(PrimaryJobId).WithCompanyId(CompanyId).Build());
        matchRepo.Seed(new MatchBuilder().WithId(MatchId).AppliedFor(ExistingUserId, PrimaryJobId).Build());

        var service = BuildService();
        var card = new JobRecommendationResult
        {
            Job = await jobRepo.GetByIdAsync(PrimaryJobId) ?? throw new InvalidOperationException(),
            Company = await companyRepo.GetByIdAsync(CompanyId) ?? throw new InvalidOperationException(),
        };

        Func<Task> act = () => service.ApplyLikeAsync(ExistingUserId, card);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Already applied*");
    }

    [Fact]
    public async Task ApplyDismissAsync_ValidCardProvided_RecordsRecommendation()
    {
        userRepo.Seed(new UserBuilder().WithId(ExistingUserId).Build());
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(PrimaryJobId).WithCompanyId(CompanyId).Build());

        var service = BuildService();
        var card = new JobRecommendationResult
        {
            Job = await jobRepo.GetByIdAsync(PrimaryJobId) ?? throw new InvalidOperationException(),
            Company = await companyRepo.GetByIdAsync(CompanyId) ?? throw new InvalidOperationException(),
        };

        var dismissId = await service.ApplyDismissAsync(ExistingUserId, card);

        (await recommendationRepo.GetByIdAsync(dismissId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoLikeAsync_IdsProvided_RemovesMatchAndRecommendation()
    {
        matchRepo.Seed(new MatchBuilder().WithId(AlternateMatchId).AppliedFor(ExistingUserId, PrimaryJobId).Build());
        recommendationRepo.Seed(new Recommendation { RecommendationId = UndoRecommendationId, UserId = ExistingUserId, JobId = PrimaryJobId });

        var service = BuildService();
        await service.UndoLikeAsync(AlternateMatchId, UndoRecommendationId);

        (await matchRepo.GetByIdAsync(AlternateMatchId)).Should().BeNull();
        (await recommendationRepo.GetByIdAsync(UndoRecommendationId)).Should().BeNull();
    }

    [Fact]
    public async Task UndoLikeAsync_RecommendationIdIsNull_SkipsRecommendationRemoval()
    {
        matchRepo.Seed(new MatchBuilder().WithId(AlternateMatchId).Build());
        recommendationRepo.Seed(new Recommendation { RecommendationId = UndoRecommendationId, UserId = ExistingUserId, JobId = PrimaryJobId });

        var service = BuildService();
        await service.UndoLikeAsync(AlternateMatchId, null);

        (await recommendationRepo.GetByIdAsync(UndoRecommendationId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_DistinctIdsProvided_RemovesDismissAndDisplayRecommendations()
    {
        recommendationRepo.Seed(
            new Recommendation { RecommendationId = UndoRecommendationId, UserId = ExistingUserId, JobId = PrimaryJobId },
            new Recommendation { RecommendationId = AlternateRecommendationId, UserId = ExistingUserId, JobId = PrimaryJobId });

        var service = BuildService();
        await service.UndoDismissAsync(UndoRecommendationId, AlternateRecommendationId);

        (await recommendationRepo.GetByIdAsync(UndoRecommendationId)).Should().BeNull();
        (await recommendationRepo.GetByIdAsync(AlternateRecommendationId)).Should().BeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_IdenticalIdsProvided_RemovesSingleRecommendation()
    {
        recommendationRepo.Seed(new Recommendation { RecommendationId = UndoRecommendationId, UserId = ExistingUserId, JobId = PrimaryJobId });

        var service = BuildService();
        await service.UndoDismissAsync(UndoRecommendationId, UndoRecommendationId);

        (await recommendationRepo.GetByIdAsync(UndoRecommendationId)).Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ExperienceBucketCases))]
    public void MapUserYearsToExperienceBucket_YearsProvided_ClassifiesYearCountIntoCorrectBucket(int years, string expected)
    {
        UserRecommendationService.MapUserYearsToExperienceBucket(years).Should().Be(expected);
    }
}
