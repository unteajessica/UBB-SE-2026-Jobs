using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class UserStatusServiceTests
{
    private readonly FakeMatchRepository matchRepo = new();
    private readonly FakeJobRepository jobRepo = new();
    private readonly FakeCompanyRepository companyRepo = new();
    private readonly FakeUserSkillRepository userSkillRepo = new();
    private readonly FakeJobSkillRepository jobSkillRepo = new();
    private readonly UserStatusService service;

    public UserStatusServiceTests()
    {
        service = new UserStatusService(
            matchRepo,
            new JobService(jobRepo),
            new CompanyService(companyRepo),
            new UserSkillService(userSkillRepo),
            new JobSkillService(jobSkillRepo));
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_UserHasNoMatches_ReturnsEmptyList()
    {
        var result = await service.GetApplicationsForUserAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_MatchHasMissingJob_SkipsInvalidMatches()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 999).Build());

        var result = await service.GetApplicationsForUserAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_ValidMatchExists_ReturnsApplicationCardWithCorrectCompanyAndScore()
    {
        companyRepo.Seed(new CompanyBuilder().WithId(5).WithName("Acme").Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder()
            .WithId(1)
            .AppliedFor(1, 10)
            .WithStatus(MatchStatus.Applied)
            .Build());
        userSkillRepo.Seed(new UserSkill { UserId = 1, SkillId = 100, Score = 80 });
        jobSkillRepo.Seed(new JobSkill { JobId = 10, SkillId = 100, RequiredLevel = 80 });

        var result = await service.GetApplicationsForUserAsync(1);

        result.Should().HaveCount(1);
        result[0].MatchId.Should().Be(1);
        result[0].JobId.Should().Be(10);
        result[0].CompanyName.Should().Be("Acme");
        result[0].CompatibilityScore.Should().Be(100);
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_JobHasNoRequiredSkills_ReturnsFullCompatibilityScore()
    {
        companyRepo.Seed(new CompanyBuilder().WithId(5).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).Build());

        var result = await service.GetApplicationsForUserAsync(1);

        result[0].CompatibilityScore.Should().Be(100);
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_CompanyIsMissing_FallsBackToUnknownCompanyName()
    {
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(99).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).Build());

        var result = await service.GetApplicationsForUserAsync(1);

        result[0].CompanyName.Should().Be("Unknown Company");
    }
}