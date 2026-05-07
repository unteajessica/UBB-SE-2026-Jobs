using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class MatchServiceTests
{
    private readonly FakeMatchRepository matchRepo = new();
    private readonly FakeJobRepository jobRepo = new();
    private readonly MatchService service;

    public MatchServiceTests()
    {
        service = new MatchService(matchRepo, new JobService(jobRepo));
    }

    [Theory]
    [InlineData(MatchStatus.Applied, MatchStatus.Accepted, true)]
    [InlineData(MatchStatus.Applied, MatchStatus.Rejected, true)]
    [InlineData(MatchStatus.Applied, MatchStatus.Advanced, true)]
    [InlineData(MatchStatus.Applied, MatchStatus.Applied, false)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Accepted, true)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Rejected, true)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Advanced, false)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Applied, false)]
    [InlineData(MatchStatus.Accepted, MatchStatus.Rejected, false)]
    [InlineData(MatchStatus.Accepted, MatchStatus.Advanced, false)]
    [InlineData(MatchStatus.Accepted, MatchStatus.Applied, false)]
    [InlineData(MatchStatus.Rejected, MatchStatus.Accepted, false)]
    [InlineData(MatchStatus.Rejected, MatchStatus.Advanced, false)]
    [InlineData(MatchStatus.Rejected, MatchStatus.Applied, false)]
    public void IsDecisionTransitionAllowed_matches_state_machine(MatchStatus from, MatchStatus to, bool allowed)
    {
        var match = new MatchBuilder().WithStatus(from).Build();

        service.IsDecisionTransitionAllowed(match, to).Should().Be(allowed);
    }

    [Fact]
    public async Task CreatePendingApplicationAsync_creates_match_in_Applied_state()
    {
        var matchId = await service.CreatePendingApplicationAsync(userId: 1, jobId: 10);

        matchId.Should().BeGreaterThan(0);
        var match = await service.GetByIdAsync(matchId);
        match!.Status.Should().Be(MatchStatus.Applied);
        match.UserId.Should().Be(1);
        match.JobId.Should().Be(10);
    }

    [Fact]
    public async Task CreatePendingApplicationAsync_throws_when_match_already_exists()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).AppliedFor(1, 10).Build());

        Func<Task> act = () => service.CreatePendingApplicationAsync(1, 10);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task SubmitDecisionAsync_accepts_applied_match()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Applied).Build());

        await service.SubmitDecisionAsync(1, MatchStatus.Accepted, "Welcome");

        var match = await service.GetByIdAsync(1);
        match!.Status.Should().Be(MatchStatus.Accepted);
        match.FeedbackMessage.Should().Be("Welcome");
    }

    [Fact]
    public async Task SubmitDecisionAsync_rejects_applied_match_with_feedback()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Applied).Build());

        await service.SubmitDecisionAsync(1, MatchStatus.Rejected, "Lacking experience");

        (await service.GetByIdAsync(1))!.Status.Should().Be(MatchStatus.Rejected);
    }

    [Fact]
    public async Task SubmitDecisionAsync_throws_when_rejecting_without_feedback()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Applied).Build());

        Func<Task> act = () => service.SubmitDecisionAsync(1, MatchStatus.Rejected, "   ");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Feedback is required*");
    }

    [Fact]
    public async Task SubmitDecisionAsync_throws_for_invalid_decision_status()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Applied).Build());

        Func<Task> act = () => service.SubmitDecisionAsync(1, MatchStatus.Applied, "x");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SubmitDecisionAsync_throws_when_transition_not_allowed()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Accepted).Build());

        Func<Task> act = () => service.SubmitDecisionAsync(1, MatchStatus.Rejected, "x");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SubmitDecisionAsync_throws_when_match_missing()
    {
        Func<Task> act = () => service.SubmitDecisionAsync(404, MatchStatus.Accepted, "x");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AdvanceAsync_moves_applied_to_advanced()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Applied).Build());

        await service.AdvanceAsync(1);

        (await service.GetByIdAsync(1))!.Status.Should().Be(MatchStatus.Advanced);
    }

    [Fact]
    public async Task AdvanceAsync_throws_when_not_applied()
    {
        matchRepo.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Advanced).Build());

        Func<Task> act = () => service.AdvanceAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RevertToAppliedAsync_resets_status_and_feedback()
    {
        matchRepo.Seed(new MatchBuilder()
            .WithId(1)
            .WithStatus(MatchStatus.Rejected)
            .WithFeedback("Sorry")
            .Build());

        await service.RevertToAppliedAsync(1);

        var match = await service.GetByIdAsync(1);
        match!.Status.Should().Be(MatchStatus.Applied);
        match.FeedbackMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCompanyIdAsync_filters_via_jobs_owned_by_company()
    {
        jobRepo.Seed(
            new JobBuilder().WithId(10).WithCompanyId(5).Build(),
            new JobBuilder().WithId(20).WithCompanyId(99).Build());
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithTimestamp(DateTime.UtcNow.AddHours(-2)).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).WithTimestamp(DateTime.UtcNow).Build(),
            new MatchBuilder().WithId(3).AppliedFor(3, 20).Build());

        var matches = await service.GetByCompanyIdAsync(5);

        matches.Should().HaveCount(2);
        matches[0].MatchId.Should().Be(2);
        matches[1].MatchId.Should().Be(1);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_returns_empty_when_company_has_no_jobs()
    {
        var matches = await service.GetByCompanyIdAsync(99);

        matches.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMatchStatisticsAsync_counts_within_each_window()
    {
        var now = DateTime.Now;
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).WithTimestamp(now.AddDays(-10)).Build(),
            new MatchBuilder().WithId(2).AppliedFor(1, 11).WithTimestamp(now.AddMonths(-3)).Build(),
            new MatchBuilder().WithId(3).AppliedFor(1, 12).WithTimestamp(now.AddMonths(-9)).Build(),
            new MatchBuilder().WithId(4).AppliedFor(1, 13).WithTimestamp(now.AddYears(-2)).Build());

        var stats = await service.GetMatchStatisticsAsync(1);

        stats.TotalMatches.Should().Be(4);
        stats.MatchesLastMonth.Should().Be(1);
        stats.MatchesLastSixMonths.Should().Be(2);
        stats.MatchesLastYear.Should().Be(3);
    }

    [Fact]
    public async Task GetMatchStatisticsAsync_groups_matches_by_position_label()
    {
        var match1 = new MatchBuilder().WithId(1).AppliedFor(1, 10).Build();
        match1.Job = new JobBuilder().WithId(10).WithRole(JobRole.BackendDeveloper).Build();
        var match2 = new MatchBuilder().WithId(2).AppliedFor(1, 11).Build();
        match2.Job = new JobBuilder().WithId(11).WithRole(JobRole.FrontendDeveloper).Build();
        matchRepo.Seed(match1, match2);

        var stats = await service.GetMatchStatisticsAsync(1);

        stats.MatchesPerPosition.Should().ContainKey("Backend");
        stats.MatchesPerPosition.Should().ContainKey("Frontend");
    }

    [Fact]
    public async Task GetMatchesForUserAsync_returns_user_matches()
    {
        matchRepo.Seed(
            new MatchBuilder().WithId(1).AppliedFor(1, 10).Build(),
            new MatchBuilder().WithId(2).AppliedFor(2, 10).Build());

        var result = await service.GetMatchesForUserAsync(1);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(1);
    }
}
