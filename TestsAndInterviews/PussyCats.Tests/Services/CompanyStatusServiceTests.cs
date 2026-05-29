using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.CompanyStatusService;
using PussyCats.Library.Services.UserSkillService;

namespace PussyCats.Tests.Services;

public class CompanyStatusServiceTests
{
    private readonly FakeMatchRepository matchRepository = new();
    private readonly FakeJobRepository jobRepository = new();
    private readonly FakeUserRepository userRepository = new();
    private readonly FakeUserSkillRepository userSkillRepository = new();
    private readonly CompanyStatusService service;

    public CompanyStatusServiceTests()
    {
        var jobService = new JobService(jobRepository);
        service = new CompanyStatusService(
            new MatchService(matchRepository, jobService, new UserService(userRepository)),
            new UserService(userRepository),
            jobService,
            new UserSkillService(userSkillRepository));
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MatchesAreNotYetDecided_ReturnsOnlyDecidedMatches()
    {
        const int firstUserId = 1, secondUserId = 2;
        const int jobId = 10, companyId = 5;
        userRepository.Seed(new UserBuilder().WithId(firstUserId).Build(), new UserBuilder().WithId(secondUserId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, jobId).WithStatus(MatchStatus.Applied).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, jobId).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);

        const int expectedNumberOfApplicants = 1, expectedApplicantId = 2;

        result.Should().HaveCount(expectedNumberOfApplicants);
        result[0].Match.MatchId.Should().Be(expectedApplicantId);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MatchesAreAdvancedOrRejected_IncludesAdvancedAndRejectedMatches()
    {
        const int firstUserId = 1, secondUserId = 2;
        const int jobId = 10, companyId = 5;
        userRepository.Seed(new UserBuilder().WithId(firstUserId).Build(), new UserBuilder().WithId(secondUserId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, jobId).WithStatus(MatchStatus.Advanced).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, jobId).WithStatus(MatchStatus.Rejected).Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);

        int expectedNumberOfApplicants = 2;
        result.Should().HaveCount(expectedNumberOfApplicants);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_UserOrJobIsMissing_SkipsMatchesWithMissingData()
    {
        const int jobId = 10, companyId = 5;
        const int nonExistentUserId = 99;
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder()
            .WithId(1)
            .AppliedFor(nonExistentUserId, jobId)
            .WithStatus(MatchStatus.Accepted)
            .Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_MultipleApplicantsExist_SortsDescendingByCompatibilityScore()
    {
        const int firstUserId = 1, secondUserId = 2;
        const string firstUserCity = "Bucharest", secondUserCity = "Bucharest, Romania";
        const int jobId = 10, companyId = 5;
        const int skillId = 1, firstUserScore = 60, secondUserScore = 90;

        userRepository.Seed(
            new UserBuilder().WithId(firstUserId).WithCity(firstUserCity).Build(),
            new UserBuilder().WithId(secondUserId).WithCity(secondUserCity).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).WithLocation("Bucharest, Romania").Build());
        userSkillRepository.Seed(
            new UserSkill { User = new User { UserId = firstUserId }, Skill = new Skill { SkillId = skillId }, Score = firstUserScore },
            new UserSkill { User = new User { UserId = secondUserId }, Skill = new Skill { SkillId = skillId }, Score = secondUserScore });
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, jobId).WithStatus(MatchStatus.Accepted).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, jobId).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);

        const int expectedNumberOfApplicants = 2;
        result.Should().HaveCount(expectedNumberOfApplicants);
        result[0].CompatibilityScore.Should().BeGreaterThan(result[1].CompatibilityScore);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_JobLocationIncludesCountry_AppliesLocationBonus()
    {
        const int userId = 1, jobId = 10, companyId = 5, skillId = 1, userSkillScore = 50;
        const string city = "Bucharest", jobLocation = "Bucharest, Romania";
        userRepository.Seed(new UserBuilder().WithId(userId).WithCity(city).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).WithLocation(jobLocation).Build());

        userSkillRepository.Seed(new UserSkill { User = new User { UserId = userId }, Skill = new Skill { SkillId = skillId }, Score = userSkillScore });

        matchRepository.Seed(new MatchBuilder().WithId(1).AppliedFor(userId, jobId).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(companyId);

        const int expectedCompatibilityScore = 60; // 50 base score + 10 location bonus

        result[0].CompatibilityScore.Should().Be(expectedCompatibilityScore);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_MatchExists_ReturnsSpecificApplicant()
    {
        const int userId = 1, jobId = 10, companyId = 5;
        const int matchId = 1;
        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(matchId).AppliedFor(userId, jobId).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantByMatchIdAsync(companyId, matchId);

        result.Should().NotBeNull();
        result!.Match.MatchId.Should().Be(matchId);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_MatchIsMissing_ReturnsNull()
    {
        const int nonExistentCompanyId = 5, nonExistentMatchId = 999;
        var result = await service.GetApplicantByMatchIdAsync(nonExistentCompanyId, nonExistentMatchId);

        result.Should().BeNull();
    }
}
