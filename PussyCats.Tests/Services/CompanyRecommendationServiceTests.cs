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
    private readonly FakeMatchRepository matchRepo = new();
    private readonly FakeJobRepository jobRepo = new();
    private readonly FakeUserRepository userRepo = new();
    private readonly FakeUserSkillRepository userSkillRepo = new();
    private readonly FakeJobSkillRepository jobSkillRepo = new();
    private readonly IRecommendationAlgorithm algorithm = Substitute.For<IRecommendationAlgorithm>();

    private CompanyRecommendationService BuildService()
    {
        var jobService = new JobService(jobRepo);
        return new CompanyRecommendationService(
            new MatchService(matchRepo, jobService),
            new UserService(userRepo),
            jobService,
            new UserSkillService(userSkillRepo),
            new JobSkillService(jobSkillRepo),
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
        userRepo.Seed(new UserBuilder().WithId(1).Build(), new UserBuilder().WithId(2).Build());
        jobRepo.Seed(
            new JobBuilder().WithId(10).WithCompanyId(5).Build(),
            new JobBuilder().WithId(20).WithCompanyId(99).Build());
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).WithStatus(MatchStatus.Rejected).Build(),
            new MatchBuilder().WithId(3).AppliedFor(1, 20).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!)
            .ReturnsForAnyArgs(50.0);

        var service = BuildService();
        await service.LoadApplicantsAsync(5);

        service.HasMore.Should().BeTrue();
        service.GetNextApplicant()!.Match.MatchId.Should().Be(1);
    }

    [Fact]
    public async Task LoadApplicantsAsync_MultipleApplicants_SortsApplicantsByScoreDescending()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build(), new UserBuilder().WithId(2).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(
            Arg.Is<User>(u => u.UserId == 1),
            Arg.Any<Job>(),
            Arg.Any<IReadOnlyList<UserSkill>>(),
            Arg.Any<IReadOnlyList<JobSkill>>()).Returns(40.0);
        algorithm.CalculateCompatibilityScore(
            Arg.Is<User>(u => u.UserId == 2),
            Arg.Any<Job>(),
            Arg.Any<IReadOnlyList<UserSkill>>(),
            Arg.Any<IReadOnlyList<JobSkill>>()).Returns(80.0);

        var service = BuildService();
        await service.LoadApplicantsAsync(5);

        var first = service.GetNextApplicant();
        first!.User.UserId.Should().Be(2);
        service.MoveToNext();
        service.GetNextApplicant()!.User.UserId.Should().Be(1);
    }

    [Fact]
    public async Task MoveToNext_QueueExhausted_ReturnsNullAfterLastApplicant()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(50.0);

        var service = BuildService();
        await service.LoadApplicantsAsync(5);

        service.MoveToNext();

        service.HasMore.Should().BeFalse();
        service.GetNextApplicant().Should().BeNull();
    }

    [Fact]
    public async Task MoveToPrevious_AtStartOfQueue_DoesNotUnderflowBelowZero()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(50.0);

        var service = BuildService();
        await service.LoadApplicantsAsync(5);

        service.MoveToPrevious();
        service.MoveToPrevious();

        service.GetNextApplicant().Should().NotBeNull();
    }

    [Fact]
    public async Task GetBreakdownAsync_Called_DelegatesToAlgorithm()
    {
        var breakdown = new CompatibilityBreakdown { OverallScore = 75 };
        algorithm.CalculateScoreBreakdown(default!, default!, default!, default!).ReturnsForAnyArgs(breakdown);

        var applicant = new UserApplicationResult
        {
            User = new UserBuilder().WithId(1).Build(),
            Match = new MatchBuilder().Build(),
            Job = new JobBuilder().WithId(10).Build(),
            UserSkills = Array.Empty<UserSkill>(),
        };

        var service = BuildService();
        var result = await service.GetBreakdownAsync(applicant);

        result!.OverallScore.Should().Be(75);
    }

    [Fact]
    public async Task ServiceInstances_MultipleCreated_DoNotShareState()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Applied).Build());
        algorithm.CalculateCompatibilityScore(default!, default!, default!, default!).ReturnsForAnyArgs(50.0);

        var serviceA = BuildService();
        var serviceB = BuildService();
        await serviceA.LoadApplicantsAsync(5);

        serviceA.HasMore.Should().Be(true);
        serviceB.HasMore.Should().Be(false);
    }
}