using FluentAssertions;
using NSubstitute;
using PussyCats.Library.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats.Library.Services.CompanyRecommendationService;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.JobSkills;
using PussyCats.Library.Services.RecommendationAlgorithm;
using PussyCats.Library.Services.UserSkillService;

namespace PussyCats.Tests.Services;

public class CompanyRecommendationServiceTests
{
    private readonly FakeMatchRepository matchRepository = new();
    private readonly FakeJobRepository jobRepository = new();
    private readonly FakeUserRepository userRepository = new();
    private readonly FakeUserSkillRepository userSkillRepository = new();
    private readonly FakeJobSkillRepository jobSkillRepository = new();
    private readonly IRecommendationAlgorithm algorithm = Substitute.For<IRecommendationAlgorithm>();

    private CompanyRecommendationService BuildService()
    {
        var jobService = new JobService(jobRepository);
        return new CompanyRecommendationService(
            new MatchService(matchRepository, jobService, new UserService(userRepository)),
            new UserService(userRepository),
            jobService,
            new UserSkillService(userSkillRepository),
            new JobSkillService(jobSkillRepository),
            algorithm);
    }

    [Fact]
    public async Task LoadApplicantsAsync_CompanyHasNoJobs_ResultsInNoApplicants()
    {
        const int nonExistentCompanyId = 99;
        var service = BuildService();

        await service.LoadApplicantsAsync(nonExistentCompanyId);

        service.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task LoadApplicantsAsync_MatchesExist_IncludesOnlyAppliedMatchesForCompanyJobs()
    {
        const int targetCompanyId = 5;
        const int otherCompanyId = 99;
        const int targetJobId = 10;
        const int otherJobId = 20;

        const int userId1 = 1;
        const int userId2= 2;

        const int appliedMatchId = 1;
        const int rejectedMatchId = 2;

        const int otherComapnyMatchId = 3;
        const double compatibilityScore = 50.0;

        userRepository.Seed(new UserBuilder().WithId(userId1).Build(), new UserBuilder().WithId(userId2).Build());
        jobRepository.Seed(
            new JobBuilder().WithId(targetJobId).WithCompanyId(targetCompanyId).Build(),
            new JobBuilder().WithId(otherJobId).WithCompanyId(otherCompanyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(appliedMatchId).AppliedFor(userId1, targetJobId).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(rejectedMatchId).AppliedFor(userId2, targetJobId).WithStatus(MatchStatus.Rejected).Build(),
            new MatchBuilder().WithId(otherComapnyMatchId).AppliedFor(userId1,otherJobId).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!)
            .ReturnsForAnyArgs(compatibilityScore);

        var service = BuildService();
        await service.LoadApplicantsAsync(targetCompanyId);

        service.GetNextApplicant()!.Match.MatchId.Should().Be(appliedMatchId);
    }

    [Fact]
    public async Task LoadApplicantsAsync_MultipleApplicants_SortsApplicantsByScoreDescending()
    {

        const int companyId = 5;
        const int jobId = 10;
        const int lowerScoringUserId = 1;
        const int higherScoringUserId = 2;
        const int matchId1 = 1;
        const int matchId2 = 2;
        const double lowScore = 40.0;
        const double highScore = 80.0;

        userRepository.Seed(new UserBuilder().WithId(lowerScoringUserId).Build(), new UserBuilder().WithId(higherScoringUserId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(matchId1).AppliedFor(lowerScoringUserId, jobId).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(matchId2).AppliedFor(higherScoringUserId, jobId).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(
            Arg.Is<User>(user => user.UserId == lowerScoringUserId),
            Arg.Any<Job>(),
            Arg.Any<IReadOnlyList<UserSkill>>(),
            Arg.Any<IReadOnlyList<JobSkill>>()).Returns(lowScore);
        algorithm.CalculateCompatibilityScore(
            Arg.Is<User>(user => user.UserId == higherScoringUserId),
            Arg.Any<Job>(),
            Arg.Any<IReadOnlyList<UserSkill>>(),
            Arg.Any<IReadOnlyList<JobSkill>>()).Returns(highScore);

        var service = BuildService();
        await service.LoadApplicantsAsync(companyId);

        var firstApplicant = service.GetNextApplicant();

        firstApplicant!.User.UserId.Should().Be(higherScoringUserId);
    }

    [Fact]
    public async Task MoveToNext_QueueExhausted_ReturnsNullAfterLastApplicant()
    {
        const int companyId = 5;
        const int jobId = 10;
        const int userId = 1;
        const int matchId = 1;
        const double compatibilityScore = 50.0;


        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, jobId).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(compatibilityScore);

        var service = BuildService();
        await service.LoadApplicantsAsync(companyId);

        service.MoveToNext();

        service.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task MoveToPrevious_AtStartOfQueue_DoesNotUnderflowBelowZero()
    {
        const int companyId = 5;
        const int jobId = 10;
        const int userId = 1;
        const int matchId = 1;
        const double compatibilityScore = 50.0;

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, jobId).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(compatibilityScore);

        var service = BuildService();
        await service.LoadApplicantsAsync(companyId);

        service.MoveToPrevious();
        service.MoveToPrevious();

        service.GetNextApplicant().Should().NotBeNull();
    }

    [Fact]
    public async Task GetBreakdownAsync_Called_DelegatesToAlgorithm()
    {
        const int overallScore = 75;
        const int userId = 1, jobId = 10;

        var breakdown = new CompatibilityBreakdown { OverallScore = overallScore };
        algorithm.CalculateScoreBreakdown(default!, default!, default!, default!).ReturnsForAnyArgs(breakdown);

        var applicant = new UserApplicationResult
        {
            User = new UserBuilder().WithId(userId).Build(),
            Match = new MatchBuilder().Build(),
            Job = new JobBuilder().WithId(jobId).Build(),
            UserSkills = Array.Empty<UserSkill>(),
        };

        var service = BuildService();
        var result = await service.GetBreakdownAsync(applicant);

        result!.OverallScore.Should().Be(overallScore);
    }

    [Fact]
    public async Task MoveToPrevious_AfterMovingNext_ReturnsToPreviousApplicant()
    {
        const int companyId = 5;
        const int jobId = 10;
        const int firstUserId = 1;
        const int secondUserId = 2;

        const int matchId1 = 1;
        const int matchId2 = 2;
        const double lowScore = 40.0;
        const double highScore = 80.0;

        userRepository.Seed(new UserBuilder().WithId(firstUserId).Build(), new UserBuilder().WithId(secondUserId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(matchId1).AppliedFor(firstUserId, jobId).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(matchId2).AppliedFor(secondUserId, jobId).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(
            Arg.Is<User>(user => user.UserId == firstUserId),
            Arg.Any<Job>(),
            Arg.Any<IReadOnlyList<UserSkill>>(),
            Arg.Any<IReadOnlyList<JobSkill>>()).Returns(highScore);
        algorithm.CalculateCompatibilityScore(
            Arg.Is<User>(user => user.UserId == secondUserId),
            Arg.Any<Job>(),
            Arg.Any<IReadOnlyList<UserSkill>>(),
            Arg.Any<IReadOnlyList<JobSkill>>()).Returns(lowScore);

        var service = BuildService();
        await service.LoadApplicantsAsync(companyId);

        var firstApplicant = service.GetNextApplicant();
        service.MoveToNext();
        service.MoveToPrevious();

        service.GetNextApplicant()!.User.UserId.Should().Be(firstApplicant!.User.UserId);
    }

    [Fact]
    public async Task ServiceInstances_MultipleCreated_DoNotShareState()
    {
        const int userId = 1, jobId = 10, companyId = 5;
        const double compatibilityScore = 50;
        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(1).AppliedFor(userId, jobId).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(compatibilityScore);

        var serviceA = BuildService();
        var serviceB = BuildService();
        await serviceA.LoadApplicantsAsync(companyId);
        serviceA.HasMore.Should().Be(true);
        serviceB.HasMore.Should().Be(false);
    }

    [Fact]
    public void HasMore_BeforeLoad_ReturnsFalse()
    {
        var service = BuildService();

        service.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task LoadApplicantsAsync_MatchWithMissingJob_IsSkipped()
    {
        const int companyId = 5;
        const int jobId = 10;
        const int missingJobId = 999;
        const int userId = 1;
        const int matchId = 1;

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, missingJobId).WithStatus(MatchStatus.Applied).Build());

        var service = BuildService();
        await service.LoadApplicantsAsync(companyId);

        service.HasMore.Should().BeFalse();
        service.GetNextApplicant().Should().BeNull();
    }
}
