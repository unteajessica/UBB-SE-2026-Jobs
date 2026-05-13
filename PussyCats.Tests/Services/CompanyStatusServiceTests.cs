using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class CompanyStatusServiceTests
{
    private readonly FakeMatchRepository matchRepo = new();
    private readonly FakeJobRepository jobRepo = new();
    private readonly FakeUserRepository userRepo = new();
    private readonly FakeUserSkillRepository userSkillRepo = new();
    private readonly CompanyStatusService service;

    public CompanyStatusServiceTests()
    {
        var jobService = new JobService(jobRepo);
        service = new CompanyStatusService(
            new MatchService(matchRepo, jobService, new UserService(userRepo)),
            new UserService(userRepo),
            jobService,
            new UserSkillService(userSkillRepo));
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MatchesAreNotYetDecided_ReturnsOnlyDecidedMatches()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build(), new UserBuilder().WithId(2).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        result.Should().HaveCount(1);
        result[0].Match.MatchId.Should().Be(2);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MatchesAreAdvancedOrRejected_IncludesAdvancedAndRejectedMatches()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build(), new UserBuilder().WithId(2).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Advanced).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).WithStatus(MatchStatus.Rejected).Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        result.Should().HaveCount(2);
    }

    /*[Fact]
    public async Task GetApplicantsForCompanyAsync_UserOrJobIsMissing_SkipsMatchesWithMissingData()
    {
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder()
            .WithId(1)
            .AppliedFor(99, 10)
            .WithStatus(MatchStatus.Accepted)
            .Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        result.Should().BeEmpty();
    }*/

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MultipleApplicantsExist_SortsDescendingByCompatibilityScore()
    {
        userRepo.Seed(
            new UserBuilder().WithId(1).WithCity("Bucharest").Build(),
            new UserBuilder().WithId(2).WithCity("Bucharest, Romania").Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).WithLocation("Bucharest, Romania").Build());
        userSkillRepo.Seed(
            new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 60 },
            new UserSkill { User = new User { UserId = 2 }, Skill = new Skill { SkillId = 1 }, Score = 90 });
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Accepted).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        result.Should().HaveCount(2);
        result[0].CompatibilityScore.Should().BeGreaterThan(result[1].CompatibilityScore);
    }

    /*[Fact]
    public async Task GetApplicantsForCompanyAsync_JobLocationIncludesCountry_AppliesLocationBonus()
    {
        userRepo.Seed(new UserBuilder().WithId(1).WithCity("Bucharest").Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).WithLocation("Bucharest, Romania").Build());

        userSkillRepo.Seed(new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 50 });

        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        result[0].CompatibilityScore.Should().Be(60);
    }*/

    [Fact]
    public async Task GetApplicantByMatchIdAsync_MatchExists_ReturnsSpecificApplicant()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantByMatchIdAsync(5, 1);

        result.Should().NotBeNull();
        result!.Match.MatchId.Should().Be(1);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_MatchIsMissing_ReturnsNull()
    {
        var result = await service.GetApplicantByMatchIdAsync(5, 999);

        result.Should().BeNull();
    }
}
