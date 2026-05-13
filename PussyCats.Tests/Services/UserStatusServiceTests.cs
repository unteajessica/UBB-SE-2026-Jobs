using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class UserStatusServiceTests
{
    private const int UserId = 1;
    private const int MissingJobId = 999;
    private const int CompanyId = 5;
    private const int MissingCompanyId = 99;
    private const int JobId = 10;
    private const int MatchId = 1;
    private const int SkillId = 100;
    private const int SkillScore = 80;
    private const int RequiredSkillLevel = 80;
    private const int FullCompatibilityScore = 100;
    private const string KnownCompanyName = "Acme";
    private const string UnknownCompanyName = "Unknown Company";

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
        var result = await service.GetApplicationsForUserAsync(UserId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_MatchHasMissingJob_SkipsInvalidMatches()
    {
        matchRepo.Seed(new MatchBuilder().WithId(MatchId).AppliedFor(UserId, MissingJobId).Build());

        var result = await service.GetApplicationsForUserAsync(UserId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_ValidMatchExists_ReturnsApplicationCardWithCorrectCompanyAndScore()
    {
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).WithName(KnownCompanyName).Build());
        jobRepo.Seed(new JobBuilder().WithId(JobId).WithCompanyId(CompanyId).Build());
        matchRepo.Seed(new MatchBuilder()
            .WithId(MatchId)
            .AppliedFor(UserId, JobId)
            .WithStatus(MatchStatus.Applied)
            .Build());
        userSkillRepo.Seed(new UserSkill { User = new User { UserId = UserId }, Skill = new Skill { SkillId = SkillId }, Score = SkillScore });
        jobSkillRepo.Seed(new JobSkill { Job = new Job { JobId = JobId }, Skill = new Skill { SkillId = SkillId }, RequiredLevel = RequiredSkillLevel });

        var result = await service.GetApplicationsForUserAsync(UserId);

        result.Should().HaveCount(1);
        result[0].MatchId.Should().Be(MatchId);
        result[0].JobId.Should().Be(JobId);
        result[0].CompanyName.Should().Be(KnownCompanyName);
        result[0].CompatibilityScore.Should().Be(FullCompatibilityScore);
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_JobHasNoRequiredSkills_ReturnsFullCompatibilityScore()
    {
        companyRepo.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepo.Seed(new JobBuilder().WithId(JobId).WithCompanyId(CompanyId).Build());
        matchRepo.Seed(new MatchBuilder().WithId(MatchId).AppliedFor(UserId, JobId).Build());

        var result = await service.GetApplicationsForUserAsync(UserId);

        result[0].CompatibilityScore.Should().Be(FullCompatibilityScore);
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_CompanyIsMissing_FallsBackToUnknownCompanyName()
    {
        jobRepo.Seed(new JobBuilder().WithId(JobId).WithCompanyId(MissingCompanyId).Build());
        matchRepo.Seed(new MatchBuilder().WithId(MatchId).AppliedFor(UserId, JobId).Build());

        var result = await service.GetApplicationsForUserAsync(UserId);

        result[0].CompanyName.Should().Be(UnknownCompanyName);
    }
}
