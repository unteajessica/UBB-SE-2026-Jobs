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
            new MatchService(matchRepo, jobService),
            new UserService(userRepo),
            jobService,
            new UserSkillService(userSkillRepo));
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_returns_only_decided_matches()
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
    public async Task GetApplicantsForCompanyAsync_includes_advanced_and_rejected_matches()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build(), new UserBuilder().WithId(2).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Advanced).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).WithStatus(MatchStatus.Rejected).Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_skips_matches_with_missing_user_or_job()
    {
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder()
            .WithId(1)
            .AppliedFor(99, 10)
            .WithStatus(MatchStatus.Accepted)
            .Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_sorts_descending_by_compatibility_score()
    {
        userRepo.Seed(
            new UserBuilder().WithId(1).WithCity("Bucharest").Build(),
            new UserBuilder().WithId(2).WithCity("Bucharest, Romania").Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).WithLocation("Bucharest, Romania").Build());
        userSkillRepo.Seed(
            new UserSkill { UserId = 1, SkillId = 1, Score = 60 },
            new UserSkill { UserId = 2, SkillId = 1, Score = 90 });
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Accepted).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        result.Should().HaveCount(2);
        result[0].CompatibilityScore.Should().BeGreaterThan(result[1].CompatibilityScore);
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_demonstrates_city_location_format_mismatch_open_item()
    {
        // OPEN ITEM: User.City "Bucharest" doesn't match Job.Location "Bucharest, Romania" — locationBonus is 0
        // even though the user is in the same city. Phase 6 should normalize.
        userRepo.Seed(new UserBuilder().WithId(1).WithCity("Bucharest").Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).WithLocation("Bucharest, Romania").Build());
        userSkillRepo.Seed(new UserSkill { UserId = 1, SkillId = 1, Score = 50 });
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantsForCompanyAsync(5);

        // No location bonus despite being the same city — bug to fix in Phase 6
        result[0].CompatibilityScore.Should().Be(50);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_returns_specific_applicant()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        jobRepo.Seed(new JobBuilder().WithId(10).WithCompanyId(5).Build());
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).WithStatus(MatchStatus.Accepted).Build());

        var result = await service.GetApplicantByMatchIdAsync(5, 1);

        result.Should().NotBeNull();
        result!.Match.MatchId.Should().Be(1);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_returns_null_when_match_missing()
    {
        var result = await service.GetApplicantByMatchIdAsync(5, 999);

        result.Should().BeNull();
    }
}
