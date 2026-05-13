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
        var service = BuildService();

        await service.LoadApplicantsAsync(99);

        service.HasMore.Should().BeFalse();
        service.GetNextApplicant().Should().BeNull();
    }

    [Fact]
    public async Task LoadApplicantsAsync_MatchesExist_IncludesOnlyAppliedMatchesForCompanyJobs()
    {
        const int firstUserId = 1, secondUserId = 2;
        const int firstCompanyId = 5, secondCompanyId = 99;
        const int firstJobId = 10, secondJobId = 20;
        const double compaitibilityScore = 50.0;
        userRepository.Seed(new UserBuilder().WithId(firstUserId).Build(), new UserBuilder().WithId(secondUserId).Build());
        jobRepository.Seed(
            new JobBuilder().WithId(firstJobId).WithCompanyId(firstCompanyId).Build(),
            new JobBuilder().WithId(secondJobId).WithCompanyId(secondCompanyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, firstJobId).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, firstJobId).WithStatus(MatchStatus.Rejected).Build(),
            new MatchBuilder().WithId(3).AppliedFor(firstUserId, secondJobId).WithStatus(MatchStatus.Applied).Build());
        
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(compaitibilityScore);

        var service = BuildService();
        await service.LoadApplicantsAsync(firstCompanyId);

        service.HasMore.Should().BeTrue();
        service.GetNextApplicant()!.Match.MatchId.Should().Be(firstUserId);
    }

    [Fact]
    public async Task LoadApplicantsAsync_MultipleApplicants_SortsApplicantsByScoreDescending()
    {
        const int firstUserId = 1, secondUserId = 2;
        const int jobId = 10, companyId = 5;
        const double compatibilityScoreForFirstUser = 40.0, compatibilityScoreForSecondUser = 80.0;
        userRepository.Seed(new UserBuilder().WithId(firstUserId).Build(), new UserBuilder().WithId(secondUserId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, jobId).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, jobId).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(
            Arg.Is<User>(user => user.UserId == firstUserId),
            Arg.Any<Job>(),
            Arg.Any<IReadOnlyList<UserSkill>>(),
            Arg.Any<IReadOnlyList<JobSkill>>()).Returns(compatibilityScoreForFirstUser);
        algorithm.CalculateCompatibilityScore(
            Arg.Is<User>(user => user.UserId == secondUserId),
            Arg.Any<Job>(),
            Arg.Any<IReadOnlyList<UserSkill>>(),
            Arg.Any<IReadOnlyList<JobSkill>>()).Returns(compatibilityScoreForSecondUser);

        var service = BuildService();
        await service.LoadApplicantsAsync(companyId);

        var first = service.GetNextApplicant();
        first!.User.UserId.Should().Be(secondUserId);
        service.MoveToNext();
        service.GetNextApplicant()!.User.UserId.Should().Be(firstUserId);
    }

    [Fact]
    public async Task MoveToNext_QueueExhausted_ReturnsNullAfterLastApplicant()
    {
        const int userId = 1;
        const int jobId = 10, companyId = 5;
        const double compatibilityScore = 50;
        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(1).AppliedFor(userId, jobId).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(compatibilityScore);

        var service = BuildService();
        await service.LoadApplicantsAsync(companyId);

        service.MoveToNext();

        service.HasMore.Should().BeFalse();
        service.GetNextApplicant().Should().BeNull();
    }

    [Fact]
    public async Task MoveToPrevious_AtStartOfQueue_DoesNotUnderflowBelowZero()
    {
        const int userId = 1, jobId = 10, companyId = 5;
        const double compatibilityScore = 50;

        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(1).AppliedFor(userId, jobId).WithStatus(MatchStatus.Applied).Build());
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
}