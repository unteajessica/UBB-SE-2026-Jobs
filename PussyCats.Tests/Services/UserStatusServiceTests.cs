using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.JobSkills;
using PussyCats_App.Services.UserSkillService;
using PussyCats_App.Services.UserStatusService;
using PussyCats.Library.Services.CompanyService;

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

    private readonly FakeMatchRepository matchRepository = new();
    private readonly FakeJobRepository jobRepository = new();
    private readonly FakeCompanyRepository companyRepository = new();
    private readonly FakeUserSkillRepository userSkillRepository = new();
    private readonly FakeJobSkillRepository jobSkillRepository = new();
    private readonly UserStatusService service;

    public UserStatusServiceTests()
    {
        service = new UserStatusService(
            matchRepository,
            new JobService(jobRepository),
            new CompanyService(companyRepository),
            new UserSkillService(userSkillRepository),
            new JobSkillService(jobSkillRepository));
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
        matchRepository.Seed(new MatchBuilder().WithId(MatchId).AppliedFor(UserId, MissingJobId).Build());

        var result = await service.GetApplicationsForUserAsync(UserId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_ValidMatchExists_ReturnsApplicationCardWithCorrectCompanyAndScore()
    {
        companyRepository.Seed(new CompanyBuilder().WithId(CompanyId).WithName(KnownCompanyName).Build());
        jobRepository.Seed(new JobBuilder().WithId(JobId).WithCompanyId(CompanyId).Build());
        matchRepository.Seed(new MatchBuilder()
            .WithId(MatchId)
            .AppliedFor(UserId, JobId)
            .WithStatus(MatchStatus.Applied)
            .Build());
        userSkillRepository.Seed(new UserSkill { User = new User { UserId = UserId }, Skill = new Skill { SkillId = SkillId }, Score = SkillScore });
        jobSkillRepository.Seed(new JobSkill { Job = new Job { JobId = JobId }, Skill = new Skill { SkillId = SkillId }, RequiredLevel = RequiredSkillLevel });

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
        companyRepository.Seed(new CompanyBuilder().WithId(CompanyId).Build());
        jobRepository.Seed(new JobBuilder().WithId(JobId).WithCompanyId(CompanyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(MatchId).AppliedFor(UserId, JobId).Build());

        var result = await service.GetApplicationsForUserAsync(UserId);

        result[0].CompatibilityScore.Should().Be(FullCompatibilityScore);
    }

    [Fact]
    public async Task GetApplicationsForUserAsync_CompanyIsMissing_FallsBackToUnknownCompanyName()
    {
        jobRepository.Seed(new JobBuilder().WithId(JobId).WithCompanyId(MissingCompanyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(MatchId).AppliedFor(UserId, JobId).Build());

        var result = await service.GetApplicationsForUserAsync(UserId);

        result[0].CompanyName.Should().Be(UnknownCompanyName);
    }
}
