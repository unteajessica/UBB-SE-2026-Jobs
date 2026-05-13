using FluentAssertions;
using NSubstitute;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats_App.Services.CooldownService;
using PussyCats_App.Services.JobService;
using PussyCats_App.Services.MatchService;
using PussyCats_App.Services.RecommendationAlgorithm;
using PussyCats_App.Services.UserRecommendationService;
using PussyCats_App.Services.UserService;
using System.Security.Cryptography;

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
        var card = await service.GetNextCardAsync(ExistingUserId, UserMatchmakingFilters.Empty());

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

        recommendationRepo.Seed(new Recommendation { User = new User { UserId = userId }, Job = new Job { JobId = recentJobId }, Timestamp = DateTime.UtcNow });
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
            User = new User { UserId = userId },
            Job = new Job { JobId = jobId },
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

        var dismissId = await service.ApplyDismissAsync(ExistingUserId, card);

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
        recommendationRepo.Seed(new Recommendation { RecommendationId = recommendationId, User = new User { UserId = userId }, Job = new Job { JobId = jobId } });

        UserRecommendationService service = BuildService();
        await service.UndoLikeAsync(matchId, recommendationId);

        (await matchRepo.GetByIdAsync(matchId)).Should().BeNull();
        (await recommendationRepo.GetByIdAsync(recommendationId)).Should().BeNull();
    }
    

    [Fact]
    public async Task UndoLikeAsync_RecommendationIdIsNull_SkipsRecommendationRemoval()
    {
        matchRepo.Seed(new MatchBuilder().WithId(AlternateMatchId).Build());
        recommendationRepo.Seed(new Recommendation { RecommendationId = UndoRecommendationId, User = new User { UserId = ExistingUserId }, Job = new Job { JobId = PrimaryJobId } });

        var service = BuildService();
        await service.UndoLikeAsync(AlternateMatchId, null);

        (await recommendationRepo.GetByIdAsync(UndoRecommendationId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_DistinctIdsProvided_RemovesDismissAndDisplayRecommendations()
    {
        int dismissId = 7;
        int displayId = 8;
        int userId = 1;
        int jobId = 10;

        recommendationRepo.Seed(
            new Recommendation { RecommendationId = dismissId, User = new User { UserId = userId }, Job = new Job { JobId = jobId }  },
            new Recommendation { RecommendationId = displayId, User = new User { UserId = userId }, Job = new Job { JobId = jobId } });

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

        recommendationRepo.Seed(new Recommendation { RecommendationId = recommendationId, User = new User { UserId = userId }, Job = new Job { JobId = jobId } });

        UserRecommendationService service = BuildService();
        await service.UndoDismissAsync(recommendationId, recommendationId);

        (await recommendationRepo.GetByIdAsync(recommendationId)).Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ExperienceBucketCases))]
    public void MapUserYearsToExperienceBucket_YearsProvided_ClassifiesYearCountIntoCorrectBucket(int years, string expected)
    {
        UserRecommendationService.MapUserYearsToExperienceBucket(years).Should().Be(expected);
    }
}
