using FluentAssertions;
using NSubstitute;
using PussyCats.Library.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.CooldownService;
using PussyCats.Library.Services.RecommendationAlgorithm;
using PussyCats.Library.Services.UserRecommendationService;
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

    private readonly FakeUserRepository userRepository = new();
    private readonly FakeJobRepository jobRepository = new();
    private readonly FakeUserSkillRepository userSkillRepository = new();
    private readonly FakeJobSkillRepository jobSkillRepository = new();
    private readonly FakeCompanyRepository companyRepository = new();
    private readonly FakeMatchRepository matchRepository = new();
    private readonly FakeRecommendationRepository recommendationRepository = new();
    private readonly IRecommendationAlgorithm algorithm = Substitute.For<IRecommendationAlgorithm>();

    private UserRecommendationService BuildService(TimeSpan? cooldownPeriod = null)
    {
        var jobService = new JobService(jobRepository);
        var matchService = new MatchService(matchRepository, jobService, new UserService(userRepository));
        var cooldown = new CooldownService(recommendationRepository, cooldownPeriod ?? TimeSpan.FromHours(CooldownHours));
        return new UserRecommendationService(
            userRepository,
            jobRepository,
            userSkillRepository,
            jobSkillRepository,
            companyRepository,
            matchService,
            recommendationRepository,
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
        userRepository.Seed(new UserBuilder().WithId(userId).Build());

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

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        companyRepository.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepository.Seed(
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

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        jobRepository.Seed(
            new JobBuilder().WithId(recentJobId).WithCompanyId(companyId).Build(),
            new JobBuilder().WithId(availableJobId).WithCompanyId(companyId).Build());
        companyRepository.Seed(new CompanyBuilder().WithId(companyId).Build());

        recommendationRepository.Seed(new Recommendation { User = new User { UserId = userId }, Job = new Job { JobId = recentJobId }, Timestamp = DateTime.UtcNow });
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

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        companyRepository.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        recommendationRepository.Seed(new Recommendation
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

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        companyRepository.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(defaultScore);

        UserRecommendationService service = BuildService();
        JobRecommendationResult? card = await service.GetNextCardAsync(userId, filters);
        int matchId = await service.ApplyLikeAsync(userId, card!);

        Match? match = await matchRepository.GetByIdAsync(matchId);
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

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        companyRepository.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, jobId).Build());

        UserRecommendationService service = BuildService();
        JobRecommendationResult card = new JobRecommendationResult
        {
            Job = await jobRepository.GetByIdAsync(jobId) ?? throw new InvalidOperationException(),
            Company = await companyRepository.GetByIdAsync(companyId) ?? throw new InvalidOperationException(),
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

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        companyRepository.Seed(new CompanyBuilder().WithId(companyId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());

        var service = BuildService();
        var card = new JobRecommendationResult
        {
            Job = await jobRepository.GetByIdAsync(jobId) ?? throw new InvalidOperationException(),
            Company = await companyRepository.GetByIdAsync(companyId) ?? throw new InvalidOperationException(),
        };

        var dismissId = await service.ApplyDismissAsync(ExistingUserId, card);

        (await recommendationRepository.GetByIdAsync(dismissId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoLikeAsync_IdsProvided_RemovesMatchAndRecommendation()
    {
        int matchId = 5;
        int recommendationId = 7;
        int userId = 1;
        int jobId = 10;

        matchRepository.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, jobId).Build());
        recommendationRepository.Seed(new Recommendation { RecommendationId = recommendationId, User = new User { UserId = userId }, Job = new Job { JobId = jobId } });

        UserRecommendationService service = BuildService();
        await service.UndoLikeAsync(matchId, recommendationId);

        (await matchRepository.GetByIdAsync(matchId)).Should().BeNull();
        (await recommendationRepository.GetByIdAsync(recommendationId)).Should().BeNull();
    }
    

    [Fact]
    public async Task UndoLikeAsync_RecommendationIdIsNull_SkipsRecommendationRemoval()
    {
        matchRepository.Seed(new MatchBuilder().WithId(AlternateMatchId).Build());
        recommendationRepository.Seed(new Recommendation { RecommendationId = UndoRecommendationId, User = new User { UserId = ExistingUserId }, Job = new Job { JobId = PrimaryJobId } });

        var service = BuildService();
        await service.UndoLikeAsync(AlternateMatchId, null);

        (await recommendationRepository.GetByIdAsync(UndoRecommendationId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_DistinctIdsProvided_RemovesDismissAndDisplayRecommendations()
    {
        int dismissId = 7;
        int displayId = 8;
        int userId = 1;
        int jobId = 10;

        recommendationRepository.Seed(
            new Recommendation { RecommendationId = dismissId, User = new User { UserId = userId }, Job = new Job { JobId = jobId }  },
            new Recommendation { RecommendationId = displayId, User = new User { UserId = userId }, Job = new Job { JobId = jobId } });

        UserRecommendationService service = BuildService();
        await service.UndoDismissAsync(dismissId, displayId);

        (await recommendationRepository.GetByIdAsync(dismissId)).Should().BeNull();
        (await recommendationRepository.GetByIdAsync(displayId)).Should().BeNull();
    }

    [Fact]
    public async Task UndoDismissAsync_IdenticalIdsProvided_RemovesSingleRecommendation()
    {
        int recommendationId = 7;
        int userId = 1;
        int jobId = 10;

        recommendationRepository.Seed(new Recommendation { RecommendationId = recommendationId, User = new User { UserId = userId }, Job = new Job { JobId = jobId } });

        UserRecommendationService service = BuildService();
        await service.UndoDismissAsync(recommendationId, recommendationId);

        (await recommendationRepository.GetByIdAsync(recommendationId)).Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ExperienceBucketCases))]
    public void MapUserYearsToExperienceBucket_YearsProvided_ClassifiesYearCountIntoCorrectBucket(int years, string expected)
    {
        UserRecommendationService.MapUserYearsToExperienceBucket(years).Should().Be(expected);
    }
}
